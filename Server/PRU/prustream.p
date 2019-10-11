//
// Arvid software and hardware is licensed under MIT license:
//
// Copyright (c) 2015 Marek Olejnik
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this hardware, software, and associated documentation files (the "Product"),
// to deal in the Product without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Product, and to permit persons to whom the Product is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Product.
//
// THE PRODUCT IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE PRODUCT OR THE USE OR OTHER DEALINGS
// IN THE PRODUCT.




#include "utils.p"

#define GPIO3 0x481ae000
#define GPIO0 0x44e07000
#define GPIO_DATAIN 0x138
#define LINE_CNT r2
#define BLOCK_COUNT r3
#define STATE	r4

#define FRAME_BUFFER r5
#define PIXEL_BUFFER r6
#define PIXEL_ADDR   r7
#define GPIO3_IN_ADDR r8
#define GPIO0_IN_ADDR r9
#define DATA r10
#define FRAME_NUM  r26
#define TMPR r27

#define STATE_ADDR 			0x10000
#define DDR_ADDR			0x10004
#define BLOCK_COUNT_ADDR 	 0x2008
#define FRAME_NUM_ADDR   	 0x2004
#define BUTTON_STATE_ADDR    0x2010
#define PIXEL_ADDR_START 	0x10010
#define FB_BLOCK_INDEX      0x10008

// To signal the host that we're done, we set bit 5 in our R31
// simultaneously with putting the number of the signal we want
// into R31 bits 0-3. See 5.2.2.2 in AM335x PRU-ICSS Reference Guide.
#define PRU0_R31_VEC_VALID (1<<5)
#define EVT_OUT_0 3 // corresponds to PRU_EVTOUT_0
#define EVT_OUT_1 4


// ****************************************
// Program start - PRU0 data streamer
// ****************************************
Start:
	//enable memory interconnect
	LBCO r0, C4, 4, 4
	CLR  r0, r0, 4
	SBCO r0, C4, 4, 4

	//set-up GPIO address - will need it to read button state
	mov GPIO3_IN_ADDR, GPIO3 | GPIO_DATAIN
	mov GPIO0_IN_ADDR, GPIO0 | GPIO_DATAIN

	//synchronization  with PRU1
	// wait until STATE is equal 0xAC
	mov r0, STATE_ADDR
init_sync:
	lbbo STATE, r0, 0, 4
	qbne init_sync, STATE, 0xAC


//set the block count
	mov r0, BLOCK_COUNT_ADDR
	lbbo BLOCK_COUNT, r0, 0, 4

// note: top 16 bits of BLOCK_COUNT contain
// frame buffer address shift!

//set up frame index
	mov FRAME_NUM, 0

// =============================================================
// prepare pixels in the buffer 
// =============================================================

//set up inital PIXEL_ADDR of the generated buffer
	mov PIXEL_ADDR, PIXEL_ADDR_START
	add PIXEL_ADDR, PIXEL_ADDR, 200	//reserve 1360 bytes (680 pixels) for the PRU1 line buffer
	add PIXEL_ADDR, PIXEL_ADDR, 200
	add PIXEL_ADDR, PIXEL_ADDR, 200
	add PIXEL_ADDR, PIXEL_ADDR, 200
	add PIXEL_ADDR, PIXEL_ADDR, 200
	add PIXEL_ADDR, PIXEL_ADDR, 200
	add PIXEL_ADDR, PIXEL_ADDR, 160


// =============================================================
// Single frame sreaming start
// =============================================================
streaming_start:


	//read gpio3 buttons into r1
	lbbo r1, GPIO3_IN_ADDR, 0, 4
	//read gpio0 button into r0
	lbbo r0, GPIO0_IN_ADDR, 0, 4

	//relocate bits 14 and 15 from r0 to bits 24 and 25
	//otherwise they would clash with gpio3 bits (stored in r1)
    lsl TMPR, r0, 10  //shift r0 right by 10 bits and store to TMPR
	and TMPR.b3, TMPR.b3, 3 // keep only these 2 bits (24,25), clear everything else

	//clear bits 14 & 15 in r0
	and r0.b1, r0.b1, 0x3F

	//combine button readings
	or r1, r1, r0
	or r1.b3, r1.b3, TMPR.b3

	//store combined button state (now in r1) to pruMem[4]
	mov r0, BUTTON_STATE_ADDR
	sbbo r1, r0, 0, 4
	
	mov r0, BLOCK_COUNT_ADDR
	lbbo BLOCK_COUNT, r0, 0, 4

	mov FRAME_BUFFER, PIXEL_ADDR	//source 

	//store the FRAME_NUM so the host can read it
	mov r0, FRAME_NUM_ADDR 
	sbbo FRAME_NUM, r0, 0, 4

	//* signal the interrupt to the streamer
	mov r31.b0, PRU0_R31_VEC_VALID | EVT_OUT_1


	//setup next frame number (back buffer host can safely write to)
	add FRAME_NUM, FRAME_NUM, 1

	mov PIXEL_BUFFER, PIXEL_ADDR_START		//destination address


// stream 320 pixel per line 
//we will read 64 bytes at once (16 registers, 32 pixels)
// note: top 16 bits of BLOCK_COUNT contain
// frame buffer address shift!
// use bottom 16 bits only

	//start from line 0 in line counter
	mov LINE_CNT, 0

	//save current line number
	mov r0, FB_BLOCK_INDEX
	sbbo LINE_CNT, r0, 0 , 4

	//* signal the interrupt to the streamer
	mov r31.b0, PRU0_R31_VEC_VALID | EVT_OUT_0

	mov r0, BLOCK_COUNT.w0 		// count 10 * 32 = 640 bytes =>320 pixels (2 bytes per pixel)


//Single line streaming start
copy_line:
	lbbo DATA, FRAME_BUFFER, 0, 64  		//read from src  cca 46 cycles

	sbbo DATA, PIXEL_BUFFER, 0, 64			//write to shared memory 6 cycles
	add FRAME_BUFFER, FRAME_BUFFER, 64		//increase memory pointers
	add PIXEL_BUFFER, PIXEL_BUFFER, 64
	sub r0, r0, 1						//decrease pixel block count

	qbne copy_line, r0, 0				//copy until the end of line

	//subtract extra bytes from frame buffer address if the frame buffer
	//width is not multiply of 32 pixels (64 bytes). Use top 16 bits of the BLOCK_COUNT.
	sub FRAME_BUFFER, FRAME_BUFFER, BLOCK_COUNT.w2

	//reset destination address
	mov PIXEL_BUFFER, PIXEL_ADDR_START

	//synchronize with PRU1
	//wait till the line has been drawn
	//basically if STATE is not 0 it means new line can be rendered
    // state value 1 means new line, state value 2 means start
  	// from the beginning of the frame
	mov r0, STATE_ADDR
line_sync:
	lbbo STATE, r0, 0, 4
	qbeq line_sync, STATE, 0x0

	//clear sync
	mov r1, 0
	sbbo r1, r0, 0, 4

	//store 3 low bits of current line number to r1
	and r1, LINE_CNT, 7

	//increase line index
	add LINE_CNT, LINE_CNT, 1

	//if the line counter is 4 or 8 (index 3 or 7) then save it
	qbeq save_line_counter_and_reset_fb, r1, 7
	qbeq save_line_counter, r1, 3

	//else jump to line finish
	qba line_finish

save_line_counter_and_reset_fb:
	// reset the mini frame buffer
	mov FRAME_BUFFER, PIXEL_ADDR	//source 

save_line_counter:
	//* this is the 4th line -> signal the interrupt to the streamer

	//save current line number (r2)
	mov r0, FB_BLOCK_INDEX
	sbbo LINE_CNT, r0, 0 , 4

	mov r31.b0, PRU0_R31_VEC_VALID | EVT_OUT_0

line_finish:
	qbeq streaming_start, STATE, 2		//state is 2 - start over new frame

	mov r0, BLOCK_COUNT.w0
	qba copy_line					//copy next line

