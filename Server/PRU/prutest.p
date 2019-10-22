#include "utils.p"

#define PRU0_R31_VEC_VALID (1<<5)
#define EVT_OUT_2 2 

// ****************************************
// Program start - PRU0 data streamer
// ****************************************
Start:
	//enable memory interconnect
	LBCO r0, C4, 4, 4
	CLR  r0, r0, 4
	SBCO r0, C4, 4, 4
	
	ldi r5, 49998

test_loop:
    mov r31.b0, PRU0_R31_VEC_VALID | EVT_OUT_2
    mov r4, r5
    
wait:
    sub r4, r4, 1
    qbne wait, r4, 0
    
    NOP
    qba test_loop