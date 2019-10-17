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



.setcallreg r26.w2
.origin 0
.entrypoint Start


// ************************************************
// Header file for PRU ASM programs
// ************************************************

// ************************************************
// NOP - no operation
// ************************************************
.macro NOP
	mov r17, r17
.endm

// ************************************************
// SAVE_RETURN_ADDRESS
// by default stores to r26.w0
// ************************************************
.macro SAVE_RETURN_ADDRESS
.mparam save_reg=r26.w0
	mov save_reg, r26.w2
.endm


// ************************************************
// RESTORE_RETURN_ADDRESS
// ************************************************
.macro RESTORE_RETURN_ADDRESS
.mparam rest_reg=r26.w0
	mov r26.w2, rest_reg
.endm

// ************************************************
// RETURN - returns (jumps) to saved address
// ************************************************
.macro RETURN
.mparam save_reg=r26.w0
	jmp save_reg
.endm


// ************************************************
// GOTO - goto label
// ************************************************
.macro GOTO
.mparam label
	QBA label
.endm

