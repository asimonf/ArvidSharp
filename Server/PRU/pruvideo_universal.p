//
// Arvid software and hardware is licensed under MIT license:
//
// Copyright (c) 2015 - 2016 Marek Olejnik
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

#define PULSE_CYCLES 56
#define PULSE_CYCLES_2X 112

#define VIDEO_0 0
#define VIDEO_1 1
#define VIDEO_2 2
#define VIDEO_3 3

#define FRAME_BUFFER r5
#define PIXEL_BUFFER r10
#define PIXEL_COLOR r11

#define GPIO1		0x4804c000
#define GPIO2		0x481ac000

//address to set individual bit on GPIO register
#define GPIO_SET	0x194

//address to clear individual bit on GPIO register
#define GPIO_CLR	0x190

//address to set and clear all bits on GPIO register
#define GPIO_DATAOUT 0x13c

#define SYNC_HI		    r7
#define SYNC_LO		    r8
#define SYNC_BIT	    r9
#define GPIO_OUT_ADDR   r12
#define GPIO_OUT_CLEAR  r13
#define LINE_POS_MOD    r14
#define VSYNC_LINE      r15
#define VSYNC_CNT       r16

#define TOTAL_LINES  r3
#define PIXEL_CNT r23.w1

#define PASSIVE_MOD_LOCATION r20.w0
#define LINE_END_DELAY_MOD_LOCATION r20.w2
#define ODD_FIELD_FLAG r24.w0
#define ASYMETRIC_PIXELS_LOCATION r24.w2

// Universal timing data registers
#define UNIVERSAL_DATA_REG		r21

#define PIXEL_AND_ASYMETRY		r21
#define PIXELS_PER_LINE_REG 	r21.w0
#define ASYMETRIC_PIXELS_REG 	r21.w2

#define PASSIVE_AND_LINE_END_DELAY			r22
#define PASSIVE_CYCLES_PER_PIXEL_REG 		r22.b0
#define PASSIVE_CYCLES_PER_PIXEL_MOD 		r22.b1
#define LINE_END_DELAY_REG 					r22.b2
#define LINE_END_DELAY_MOD 					r22.b3

#define ENABLE_INTERLACE r23.w0

// To signal the host that we're done, we set bit 5 in our R31
// simultaneously with putting the number of the signal we want
// into R31 bits 0-3. See 5.2.2.2 in AM335x PRU-ICSS Reference Guide.
#define PRU0_R31_VEC_VALID (1<<5)
#define EVT_OUT_2 2 // corresponds to PRU_EVTOUT_2

// ****************************************
// Program start
// ****************************************
Start:
	LBCO r0, C4, 4, 4
	CLR  r0, r0, 4
	SBCO r0, C4, 4, 4

//setup sync data
	mov SYNC_BIT, 1 << 24 //GPIO2_24

//sync_lo is now address to CLEAR individual bits on GPIO
	mov SYNC_LO, GPIO2 | GPIO_CLR

//sync_hi is now address to SET individual bits on GPIO
	mov SYNC_HI, GPIO2 | GPIO_SET

//set up the gpio2 slow address. By writing to this address
//all GPIOs bits will be set and unset all at once
	mov GPIO_OUT_ADDR, GPIO2 | GPIO_DATAOUT

	mov GPIO_OUT_CLEAR, 0

// read DDR address into r4 from the data ram
// the addres was set up by the host program
	mov r0, 0
	lbbo r4, r0, 0, 4

// load total lines
	mov r0, 0xC //address 12 (3rd int index)
	lbbo TOTAL_LINES, r0, 0, 4

// load line X pos
	mov r0, 0x14 //address 20 (5th int index)
	lbbo LINE_POS_MOD, r0, 0, 4
	
// load Vsync line
	mov r0, 0x18 //address 24 (6th int index)
	lbbo VSYNC_LINE, r0, 0, 4

//set frame buffer address (ddr + 64 reserved bytes )
	mov FRAME_BUFFER, r4
	add FRAME_BUFFER, FRAME_BUFFER, 64

// write frame buffer address to shared memory
	mov r0, 0x10004
	sbbo FRAME_BUFFER, r0, 0, 4

// initialise pixel counter
	mov PIXEL_CNT, 0

// Setup Interlacing

	mov ENABLE_INTERLACE, 0
	mov ODD_FIELD_FLAG, 0
	
// send initial sync to PRU0
	mov r0, 0x10000
	mov r1, 0xac
	sbbo r1, r0, 0, 4

// wait to ensure PRU0 is synced
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP

// frame buffer now points to the PRU Shared  memory
	mov FRAME_BUFFER, 0x10010
	mov PIXEL_BUFFER, FRAME_BUFFER

// now reset PRU0 state to 0 - indicate to wait till the line
// has finished rendering
	mov r1, 0;
	sbbo r1, r0, 0, 4

// =============================================================
// INFO
// =============================================================
// you will see a lot of comments saying: comps. 1st c etc.
// That means: compensate 1st cycle, 2nd cycle etc.
// Explanation:
// We have to send synchronisation pulses at the beginning
// and end of the pixel line. These pulses have to be exactly the
// same length. But because we do some data setting and calls
// before actually doing the pulse, the pulse time might not
// be exactly the same length as other pulses. So in order to 
// ensure same pulse length we wait exactly 3 cycles before
// starting each pulse. The 'wait' of 3 cycles can be any 
// combination of instructions that together take 3 processor
// cycles - either NOP or MOV etc. PRUs usually execute
// 1 instruction in 1 cycle. Only memory access takes 4 cycles
// =============================================================

// =============================================================
// Frame start
// =============================================================
Frame:
	//Note - call instruction takes 1 cycle
	
	//Bottom sync seqence -> 6x Sync_A pattern
	call Sync_A
	call Sync_A
	call Sync_A
	call Sync_A
	call Check_Sync_A					// Check if an odd field needs to be drawn
	call Sync_A

	//Top sync sequence -> 5x Sync_B, then 5x Sync_A pattern
	call Sync_B
	call Sync_B
	call Sync_B
	call Sync_B
	call Sync_B

	call Sync_A
	call Sync_A
	call Sync_A
	call Sync_A
	call Sync_A
	GOTO lines_loop 					// comp for last pulse

Odd:
	call Sync_B
	call Sync_B
	call Sync_B
	call Sync_B
	call Sync_B

	call Sync_A
	call Sync_A
	call Sync_A
	call Sync_A
	NOP 								// comp for last pulse
	
lines_loop:
	sbbo SYNC_BIT, SYNC_LO, 0, 4	//send LO sync signal
	sub r6.w0, r6.w0, 1				// decrease loop counter -> comps. 2nd c
	call PixelLine					// call pixel drawing function

	//setup pixel buffer to point at the start of the frame buffer
	mov PIXEL_BUFFER, FRAME_BUFFER

	qbne lines_loop, r6.w0, 0   		// end of loop - comps. for call instruction


	//end of single frame

	GOTO Frame							//draw new frame


// =============================================================
// Draw pixel line
// =============================================================
PixelLine:
	// 4us of LO sync signal

	//note: LO signal has already been sent - comps. 1st c
	//note: decreasing loop counter in the caller code - comps. 2nd c
	SAVE_RETURN_ADDRESS				// comps. 3rd c
	call Pulse						// 1st LOW sync signal

	NOP								// comps. 1st c
	NOP								// comps. 2nd c
	call Pulse						// 2nd LOW sync pulse (now 4us in total)
	NOP								// comps. 3rd cycle

	// 8us of HI sync signal (2us + 3x 2us)
	//!!! SBBO 0,4 might take 4 cycles instead of 1 !!!!
	sbbo SYNC_BIT, SYNC_HI, 0, 4	// send HI sync signal, comps. 1st c !!! VERIFY number of cycles for SBBO!!!

	call Pulse						// wait 2us (1st HI sync signal)
	NOP								// comps. 2nd cycle

	mov r19.w2, 3 					// iterate 3x,  comps. 3rd cycle


pixel_line_sync_loop:
	sub r19.w2, r19.w2, 1			// decrease loop cont, comps. 1st c
	call Pulse						// wait 2us (2nd, 3rd and 4th HI sync) 
	NOP								// comps. 2nd cycle
	qbne pixel_line_sync_loop, r19.w2, 0 //iterate, comps. 3rd cycle

// NOW Draw the pixels !!!
// There is 52us time to draw the pixels, which is 26 pulses.
// The first pulse and the last are outside of visible area and should be black color.
// We can use that black space as a wiggle room to move the whole picture left
// or right according to the user preferences (value in LINE_POS_MOD).
// That means we have 24 pulses (48us) to draw pixels in whatever horizontal
// resolution we want. To draw 480 pixels across, one pixel must be set for 100ns.
// 48us / 480pixels = 0.1us = 100ns = 20 instruction cycles. Remember PRU runs
// on 200 MHz that means 1 instruction cycle takes 5ns. Now 100ns per pixel
// divided by 5ns per processor cycle gives total 20 cycles to handle single pixel
// in that resolution. For resolution 320 pixels one pixel takes 30 instructions
// to draw (48us / 320 = 0.15us = 150ns = 30 cycles).

// Handling one pixel involves:
// * reading pixel color from memory into a register (4 cycles)
// * writing pixel to GPIO register to produce analog signal (1 cycle)
// * increasing pixel index (column) - ie. jumping to the next pixel (1 cycle)
// * checking whether the last pixel was drawn -> finish (1 cycle)
// * jump to draw next pixel (1 cycle)
// In total it is 8 cycles per pixel, so in theory PRU should be able to push ~1200 pixels
// horizontally.


	//Pulse 1 - black color
	call ModPulseStart				// wait ~ 2us (depends on user preferences)
	NOP								// comps. 1st cycle
	NOP								// comps. 2nd cycle
	NOP								// comps. 3rd cycle

	//Pulse 2 to 24 ->draw real pixels

	//draw X number of pixels defiend for current resolution
	mov r0.w0, PIXELS_PER_LINE_REG

pixel_line_pixel_loop:
	//read pixel
	lbbo PIXEL_COLOR, PIXEL_BUFFER, 0, 2 		// ? 3 or 4  cycles

	//gpio2_0 pin doesn't exist on BBB, so we have to
	//shift the pixel color value left by 1 bit to start at gpio2_1 pin.
	lsl  PIXEL_COLOR, PIXEL_COLOR, 1

	sbbo PIXEL_COLOR, GPIO_OUT_ADDR, 0, 2

	//jump to next pixel (increase memory addr by 2 bytes)
	add PIXEL_BUFFER, PIXEL_BUFFER, 2			// ? 1 cycle, total 6 c

	//reduce number of pixel left to draw
	sub r0.w0, r0.w0, 1							// ? 1 cycle. total 7 c

	//wait X passive cycles (depends on horizontal resolution, different value for each videomode)
	mov r0.w2, PASSIVE_CYCLES_PER_PIXEL_REG			// delay 1 c , total 8 c
pixel_line_pixel_delay:
	sub r0.w2, r0.w2, 1
	qbeq pixel_line_pixel_delay_adjust, r0.w2, 0 	// Need to compensate 2 cycles if this is taken
	NOP
	qbne pixel_line_pixel_delay, r0.w2, 0

pixel_line_pixel_delay_adjust:
	jmp PASSIVE_MOD_LOCATION 						// compensate 1st cycle of delay adjust

	//final passive cycles fine-tuning
	NOP
	NOP
	NOP
	
pixel_line_pixel_delay_finish:
	jmp ASYMETRIC_PIXELS_LOCATION					// compensate 2nd cycle of delay adjust

//odd pixels are slightly wider
asymetric_pixels_2:
	qbbs pixel_short, r0, 0
	qba pixel_short									

//every nth pixel is slightly wider
asymetric_pixels_n:
	add PIXEL_CNT, PIXEL_CNT, 1
	qbne pixel_short, PIXEL_CNT, ASYMETRIC_PIXELS_REG
	mov PIXEL_CNT, 0

pixel_short:
	// check all pixels were drawn 
	qbne pixel_line_pixel_loop, r0.w0 , 0 		// ? 1 cycle. total 6 c

	//send sync info to PRU1 the line drawing finished
	mov r0, 0x10000

	qbeq send_sync_pulse_2, r6.w0 , 0 //last line to draw

	mov r1, 1
	qba send_pulse_continue

send_sync_pulse_2:
	mov r1, 2
	NOP

send_pulse_continue:
	sbbo r1, r0, 0, 4

//  send BLACK to GPIO port (clear colors)
	sbbo GPIO_OUT_CLEAR, GPIO_OUT_ADDR, 0, 2

// Final 2 Black pulses (bars)
// Instead of these 2 black pulses we could do something clever.
// For example set different palette for the next line (copper effects anyone ? :)
// do some line scrolling, or mix sound channels or caluclate sprite collisions etc.
// Or send Hsync notification.
// Important: we have to maintain proper timig! That means whatever we do here at first
// the black (or background) color has to be set, then total time of 4us must be kept.

	//Pulse 25
	call Pulse						            // wait 2us
	NOP	                        	            // comps. 1st cycle
	NOP								            // comps. 2nd cycle
	NOP								            // comps. 3rd cycle

	//Pulse 26
	call ModPulseEnd				            // wait ~2us (is modified via LINE_POS_MOD)
	add VSYNC_CNT, VSYNC_CNT, 1                 // comps. 1st cycle
	qbeq trigger_vsync, VSYNC_CNT, VSYNC_LINE   // comps. 2nd cycle   
	qba check_finished                          // comps. 3rd cycle (if branch not taken)
	
trigger_vsync:
    mov r31.b0, PRU0_R31_VEC_VALID | EVT_OUT_2  // comps. 3rd cycle (if branch taken)
	
check_finished:
//final delay at the end of the line (240 - 320) cycles (depending on the horiz. resolution)
	mov r0.w2, LINE_END_DELAY_REG
pixel_line_final_delay:
	sub r0.w2, r0.w2, 1
	NOP
	qbeq pixel_line_final_delay_adjust, r0.w2, 0
	qbne pixel_line_final_delay, r0.w2, 0

pixel_line_final_delay_adjust:
	jmp LINE_END_DELAY_MOD_LOCATION // Basically jump to the address at the label with a precalculated substraction
	//final delay fine-tuning
	NOP
	NOP
	NOP
	mov PIXEL_CNT, 0
	
pixel_line_final_delay_finish:
	RETURN						// return to saved address


// =============================================================
// Synchronisation pattern A: 32 us
//    __________________________
//  2 |/////// 30 //////////////
// ___|/////// us //////////////
// =============================================================

Check_Sync_A:
	sbbo SYNC_BIT, SYNC_LO, 0, 4			// send LO sync signal, comps. 1st c

	SAVE_RETURN_ADDRESS						// comps. 2nd c
	call Pulse2								// wait 2us
	mov r6.w0, TOTAL_LINES					// save all the lines of the current mode to the register

	sbbo SYNC_BIT, SYNC_HI, 0, 4 			// send hi sync signal, 3 cycles, right?

	// I need to comp 396 cycles

	// load universal data (6 cycles)
	mov r0, 0x1C 							// address 28 (7th int index)
	lbbo UNIVERSAL_DATA_REG, r0, 0, 12		// 5 cycles
	
	// Calculate Line End Delay (2 cycles long section)
	ldi LINE_END_DELAY_MOD_LOCATION, pixel_line_final_delay_finish
	sub LINE_END_DELAY_MOD_LOCATION, LINE_END_DELAY_MOD_LOCATION, LINE_END_DELAY_MOD

    // Calculate Asymmetry Jump Locations (5 cycles long section always)
	qbeq asymetry_adjust_4, ASYMETRIC_PIXELS_REG, 4 // must 4 cycles long if taken
	qbeq asymetry_adjust_3, ASYMETRIC_PIXELS_REG, 3 // must 3 cycles long if taken
	qbeq asymetry_adjust_2, ASYMETRIC_PIXELS_REG, 2 // must 2 cycles long if taken
	
	// No asymetric pixels, so just skip the whole section
	
	ldi ASYMETRIC_PIXELS_LOCATION, pixel_short
	qba calculate_passive_delay

asymetry_adjust_4:
	NOP
	NOP
	ldi ASYMETRIC_PIXELS_LOCATION, asymetric_pixels_n
	qba calculate_passive_delay

asymetry_adjust_3:
	sub LINE_END_DELAY_MOD_LOCATION, LINE_END_DELAY_MOD_LOCATION, 1 // Must clear PIXEL_CNT to 0 every line
	ldi ASYMETRIC_PIXELS_LOCATION, asymetric_pixels_n
	qba calculate_passive_delay

asymetry_adjust_2:
	NOP
	ldi ASYMETRIC_PIXELS_LOCATION, asymetric_pixels_2	

	// Calculate Passive Pixels Delay (3 cycles long section)
calculate_passive_delay:
	NOP	// Added to make the number of cycles even to compensate for the granularity of the loop below
	ldi PASSIVE_MOD_LOCATION, pixel_line_pixel_delay_finish
	sub PASSIVE_MOD_LOCATION, PASSIVE_MOD_LOCATION, PASSIVE_CYCLES_PER_PIXEL_MOD
	

	// clear ODD_FIELD_FLAG if interlacing is disabled
	and ODD_FIELD_FLAG, ENABLE_INTERLACE, ODD_FIELD_FLAG
	
	// clear VSYNC_CNT to 0
	ldi VSYNC_CNT, 0                		

	mov r19.w2, 187           				// Mark a default wait of 187 * 2 = 386 cycles for the full 1.995us
	qbeq no_interlace, ENABLE_INTERLACE, 0

	add r6.w0, r6.w0, 1						// When interlacing is enabled, an additional line must be sent
	mov r19.w2, 185           				// Mark a default wait of 185 * 2 = 382 cycles for the full 1.995us
	not ODD_FIELD_FLAG, ODD_FIELD_FLAG		// Flip the odd field flag to compensate
	qbne no_interlace, ODD_FIELD_FLAG, 0	// if ODD_FIELD_FLAG is set, it was unset before the flip so we must draw the even field

	// Must draw odd field so must wait the full 29.995us remaining in the current pulse
	mov r19.w2, 2985						// Wait 2985 * 2 = 2986 cycles for the full 29.995 us
interlace_pulse_loop:
	sub r19.w2, r19.w2, 1
	qbne interlace_pulse_loop, r19.w2, 0
	GOTO Odd

no_interlace:
	// I need to wait r19.w2 * 2 cycles to complete 1.995us
	sub r19.w2, r19.w2, 1
	qbne no_interlace, r19.w2, 0

	mov r19.w2, 13 					// iterate 14x,  comps. 3rd cycle
	GOTO sync_a_loop

Sync_A:
	sbbo SYNC_BIT, SYNC_LO, 0, 4	// send LO sync signal, comps. 1st c

	SAVE_RETURN_ADDRESS				// comps. 2nd c
	call Pulse2						// wait 2us
	NOP

	sbbo SYNC_BIT, SYNC_HI, 0, 4 	// send hi sync signal
	NOP 							// comps. for call intruction
	call Pulse2						// wait 2us


	mov r19.w2, 13 					// iterate 14x,  comps. 3rd cycle

sync_a_loop:
	NOP								// comps. 1st c
	call Pulse						// wait 2us

	sub r19.w2, r19.w2, 1			// comps. 2nd c
	qbne sync_a_loop, r19.w2, 0		// comps. 3rd c

	// 15th iteration
	NOP 							// comps. 1st c
	call Pulse						// wait 2us

	RETURN							// return to saved address, comps. 2nd c
	//Note: 3rd compensation is done by initial CALL instruction


// =============================================================
// Synchronisation pattern B: 32 us
//                       ______
//          30us         |/ 2 /
// ______________________|/////
// =============================================================

Sync_B:
	sbbo SYNC_BIT, SYNC_LO, 0, 4	// send LO sync signal, comps. 1st c

	SAVE_RETURN_ADDRESS				// comps. 2nd c
	call Pulse2						// wait 2us
	mov r19.w2, 14 					// iterate 14x,  comps. 3rd cycle

sync_b_loop:
	NOP								// send LO sync signal, comps. 1st c
	call Pulse						// wait 2us
	sub r19.w2, r19.w2, 1			// comps. 2nd c
	qbne sync_b_loop, r19.w2, 0		// comps. 3rd c

	// 15th iteration (16 th in total - the last one)
	sbbo SYNC_BIT, SYNC_HI, 0, 4	// send HI sync signal, comps. 1st c
	call Pulse2						// wait 2us

	RETURN							// return to saved address, comps. 2nd c
	//Note: 3rd compensation is done by initial CALL instruction

// =============================================================
// Pulse - waits for a period of a single pulse
// 1.980 us + (20 ns reserved, 1 cycle for call instruction itself
// extra 3 cycles must be compensated)
// 1980 / 5 => 396 cycles
// 56 (PULSE_CYCLES) * 7 cycles (sub, 5xNOP, qbne) => 392 cycles
// + extra 4 cycles => 396 cycles (mov, 2xNOP, ret) 
// =============================================================
Pulse:
	mov r19.w0, PULSE_CYCLES
pulse_loop:
	sub r19.w0, r19.w0, 1
	NOP
	NOP
	NOP
	NOP
	NOP

	qbne pulse_loop, r19.w0, 0
	NOP
	NOP
	ret

//slightly shorter puls - 3 cycles
Pulse2:
	mov r19.w0, PULSE_CYCLES -1
pulse2_loop:
	sub r19.w0, r19.w0, 1
	NOP
	NOP
	NOP
	NOP
	NOP

	qbne pulse2_loop, r19.w0, 0
	NOP
	NOP

	NOP
	NOP
	NOP
	NOP
	ret


// shorter loop - to compensate
Pulse_short:
	mov r19.w0, PULSE_CYCLES - 2
pulse_short_loop:
	sub r19.w0, r19.w0, 1
	NOP
	NOP
	NOP
	NOP
	NOP

	qbne pulse_short_loop, r19.w0, 0
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	ret

// Modified pulse - start of the pixel line (black border on the left)
ModPulseStart:
	mov r19.w0, LINE_POS_MOD
mod_pulse_start_loop:
	sub r19.w0, r19.w0, 1
	NOP
	NOP
	NOP
	NOP
	NOP
	qbne mod_pulse_start_loop, r19.w0, 0
	NOP
	NOP
	ret

ModPulseEnd:
// Modified pulse - end of the pixel line (black border on the right)
	mov r19.w0, PULSE_CYCLES_2X
	sub r19.w0, r19.w0, LINE_POS_MOD
mod_pulse_end_loop:
	sub r19.w0, r19.w0, 1
	NOP
	NOP
	NOP
	NOP
	NOP
	qbne mod_pulse_end_loop, r19.w0, 0
	NOP
// note: one NOP is missing to compensate starting SUB instruction
	ret



