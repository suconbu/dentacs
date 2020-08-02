# dentacs

The dentacs is text-box based calculator for Windows Desktop.  
It's developing with WPF and C# .NET Framework 4.7.2.

Executable binaries are available:  
https://github.com/suconbu/dentacs/releases

Demo:  

![screenshot](image/demo1.gif)

Main features:  
* Multiline expression editor
* Can be mix different radix numbers in a expression
* Display calculation result in 3 different radix numbers at once
* Character code information (CodePoint, UTF-16, UTF-8) is displayed in status bar

Motivations to develop this app:  
When I need to do a little calculation at the work, I wanted to do calculate as easy as writing expressions like as source code, but couldn't find such an calculator app.  
Also, I have been using with WinForms so far, so I wanted to create a WPF app.  

## Manual

### Shortcut keys

Key               | Description
------------------|------------------------
Tab               | Toggle keypad
F11               | Toggle full-screen mode
Ctrl + MouseWheel | Zooming text
Ctrl + A          | Select all lines
Ctrl + Z          | Undo
Ctrl + Y          | Redo

### Numbers

Numbers can be integer and floating point, which are usually not distinct.  
(but bitwise operations can only use integers)  
The integer part can represent a signed 64-bit, and the fraction part in floating point can represent up to 16 digits.  

Numerical values can be expressed in follows.  
Only decimal numbers can have fraction part.  

Type        | Representation of '1234'
------------|-------------------
Decimal     | 1234, 1234.0
Hexadecimal | 0x04d2, 0x4D2
Octal       | 0o2322
Binary      | 0b0000010011010010

### Operators

Operation           | Token | Examples
--------------------|-------|-------------------
Addition            | +     | 5 + 3  -> 8
Subtraction         | -     | 5 - 3  -> 2
Multiplication      | *     | 5 * 3  -> 15
Division            | /     | 5 / 3  -> 1.666...
FloorDivision       | //    | 5 // 3 -> 1
Modulo              | %     | 5 % 3  -> 2
Exponentiation      | **    | 5 ** 3 -> 125
Bitwise AND         | &     | 5 & 3  -> 1 (0b0101 & 0b0011 -> 0b0001)
Bitwise OR          | \|     | 5 \| 3  -> 7 (0b0101 \| 0b0011 -> 0b0111)
Bitwise XOR         | ^     | 5 ^ 3  -> 6 (0b0101 ^ 0b0011 -> 0b0110)
Bitwise NOT         | ~     | ~5     -> -6 (~0b0101 -> 0b1111...1010)
Bitwise left shift  | <<    | 5 << 3 -> 40 (0b0101 << 3 -> 0b00101000)
Bitwise right shift | >>    | 5 >> 3 -> 0  (0b0101 >> 3 -> 0b0000)
Assignment          | =     | x = 5 + 3

### Variables

Variable can store calculated value temporary.  
When you delete the expression of assignment, the variable value also delete.  
Constants, functions and keywords cannot be used variable names.  

```py
x = 10     # x:10
y = x ** 2 # y:100

PI = 3  # Error: 'PI' is a constant
pi = 3  # OK: It is case sensitive
```

### Constants

Identifier | Description
-----------|----------------------------------------------------------------------
PI         | The ratio of the circumference of a circle to its diameter (3.141...)
E          | The base of natural logarithms (2.718...)

### Keywords

The following identifiers are reserved words and cannot be used as variable names.

```
if        elif      else      then      for
in        to        repeat    do        end
continue  break     exit      or        and
not
```

## Lisence

MIT License
