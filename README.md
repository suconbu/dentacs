# dentacs

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/suconbu/dentacs)](https://github.com/suconbu/dentacs/releases)
[![Build Status](https://suconbu.visualstudio.com/dentacs/_apis/build/status/suconbu.dentacs?branchName=master)](https://suconbu.visualstudio.com/dentacs/_build/latest?definitionId=2&branchName=master)

The dentacs is text-box based calculator for Windows Desktop.  
It's developing with WPF and C# .NET Framework 4.7.2.

Executable binaries are available:  
https://github.com/suconbu/dentacs/releases

## Table of contents

* [Demo](#Demo) | [Features](#Features)
* [Shortcut keys](#Shortcut-keys)
* [Numbers](#Numbers) | [DateTime and TimeSpan](#DateTime-and-TimeSpan)
* [Operators](#Operators)
* [Variables](#Variables) | [Functions](#Functions) | [Constants](#Constants)
* [Reserved keywords](#Reserved-keywords)
* [License](#License)

## Demo

![screenshot](image/demo1.gif)

![screenshot](image/demo2.gif)

## Features

* Multiline expression editor
* Can be mix different radix numbers in a expression
* Display calculation result in 3 different radix numbers at once
* Character code information (CodePoint, UTF-16, UTF-8) is displayed in status bar
* Operation for DateTime and TimeSpan -> [DateTime and TimeSpan](#DateTime-and-TimeSpan)

## Shortcut keys

Key               | Description
------------------|------------------------
Tab               | Toggle keypad
F11               | Toggle full-screen mode
Ctrl + MouseWheel | Zooming text
Ctrl + A          | Select all lines
Ctrl + Z          | Undo
Ctrl + Y          | Redo

## Numbers

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

## DateTime and TimeSpan

DateTime and TimeSpan describe as quoted string.  
Double quotes (`"`) or single quotes (`'`) can be used as the quotation mark.

### DateTime

DateTime represents a point on the timeline.
```
----------x----------> Passage of time
          ^          
       DateTime   
```

There are following types of specification methods.

1. [General style](#DateTime---General-style)
2. [Standard style](#DateTime---Standard-style)
3. [Week number style](#DateTime---Week-number-style)

#### DateTime - General style

This is a general notation with separated by a slash and colons.  
The year and date part can be omitted, in which case the current year/day will be used.

Input                 | Result
----------------------|----------------------
'2019/8/18 7:36:13'   | '2019/08/18 07:36:13'
'2019/8/18 7:36'      | '2019/08/18 07:36:00'
'2019/8/18 7'         | '2019/08/18 07:00:00'
'2019/8/18'           | '2019/08/18 00:00:00'
'2019/8'              | '2019/08/01 00:00:00'
'08/18 7:36:13'       | '2020/08/18 07:36:13'
'7:36:13'             | '2020/04/01 07:36:13'

#### DateTime - Standard style

This style is according as ISO8601.  
The year and date part cannot be omitted in this style.

Input                      | Result
---------------------------|----------------------
'20190818T073613+0700'     | '2019/08/18 00:36:13'
'20190818T073613'          | '2019/08/18 07:36:13'
'20190818T0736'            | '2020/04/01 07:36:00'
'20190818T07'              | '2020/04/01 07:00:00'
'2019-08-18T07:36:13+7:00' | '2019/08/18 00:36:13'
'2019-08-18T07:36:13'      | '2019/08/18 07:36:13'
'2019-08-18T07:36'         | '2019/08/18 07:36:00'
'2019-08-18T07'            | '2019/08/18 07:00:00'
'2019-08-18'               | '2019/08/18 00:00:00'
'2019-08'                  | '2019/08/01 00:00:00'

#### DateTime - Week number style

Specify the date by the week number of the year.  
'CW01' is the week with the year's first Thursday in it.  
The fraction part represents the day of week, begening at 1:Monday and ending at 7:Sunday.  
If the year part is omitted, the current year will be used.  

Input                 | Result
----------------------|----------------------
'CW33.7/2019'         | '2019/08/18 00:00:00'
'CW33.7'              | '2020/08/16 00:00:00'
'CW33/2019'           | '2019/08/12 00:00:00'
'CW33'                | '2020/08/10 00:00:00'

### TimeSpan

TimeSpan represents the length between two points on the timeline.  
TimeSpan can be negative value.
```
-----x================x---x============x-----> Passage of time
     -----TimeSpan---->   <--TimeSpan---
         (Positive)         (Negative)
```

There are following types of specification methods.

1. [Comma separated style](#TimeSpan---Comma-separated-style)
2. [Unit specified style](#TimeSpan---Unit-specified-style)

#### TimeSpan - Comma separated style

This style must start with a sign (+/-).  
If the value is less than 24 hours, can be omit the date part.

Input                          | Result
-------------------------------|-------------------
'+7:36:13.123'                 | '+7:36:13.123'
'-7:36:13'                     | '-07:36:13'
'+7:36'                        | '+07:36:00'
'+7:96'                        | '+08:36:00'
'+1d 7:36:13'                  | '+1d 07:36:13'

#### TimeSpan - Unit specified style

Specify the value as combination of numbers and units.  
Can be use fraction part and negative number in each unit.

Input                          | Result
-------------------------------|-------------------
'1w1d7h36m13s123ms'            | '+8d 7:36:13.123'
'1week 1day 7hour 36min 13sec' | '+8d 7:36:13'
'1.5h 300sec'                  | '+01:35:00'
'1day -4hour 30min -10sec'     | '+20:29:50'

This style spoorts the following units.

Unit        | Specifier
------------|----------
Week        | 'week', 'w'
Day         | 'day', 'd'
Hour        | 'hour', 'h'
Minute      | 'minute', 'min', 'm'
Second      | 'second', 'sec', 's'
Millisecond | 'millisecond', 'msec', 'ms'

### Supported operations

Left hand side | Operator   | Right hand side | Result
---------------|------------|-----------------|-------
DateTime       | -          | DateTime        | TimeSpan
DateTime       | +, -       | TimeSpan        | DateTime
TimeSpan       | +, -       | TimeSpan        | TimeSpan
TimeSpan       | *, /       | Number          | TimeSpan

Example of use:
```py
'2000/4/1 12:00' - '1999/4/1 12:00'  # '366d 00:00:00'
'2000/4/1 12:00' + '+15:00'           # '2000/04/02 03:00:00'
'4/1'   + '40day'                    # '2020/05/11 00:00:00' (In 2020)
'15:00' - '300min'                   # '10:00:00'
'3day'  - '10.5h'                    # '2d 13:30:00'
'1day'  * 1.5                        # '+1d 12:00:00'
```

## Operators

The followings the supported operators in dentacs, from highest to lowest precedence.

Precedence | Operation           | Token | Examples
-----------|---------------------|-------|-------------------
1          | Exponentiation      | **    | 5 ** 3 -> 125
2          | Positive sign       | +     | +5     -> 5
2          | Negative sign       | -     | -5     -> -5
2          | Bitwise NOT         | ~     | ~5     -> -6 (~0b0101 -> 0b1111...1010)
3          | Multiplication      | *     | 5 * 3  -> 15
3          | Division            | /     | 5 / 3  -> 1.666...
3          | FloorDivision       | //    | 5 // 3 -> 1
3          | Reminder            | %     | 5 % 3  -> 2
4          | Addition            | +     | 5 + 3  -> 8
4          | Subtraction         | -     | 5 - 3  -> 2
5          | Bitwise left shift  | <<    | 5 << 3 -> 40 (0b0101 << 3 -> 0b00101000)
5          | Bitwise right shift | >>    | 5 >> 3 -> 0  (0b0101 >> 3 -> 0b0000)
6          | Bitwise AND         | &     | 5 & 3  -> 1 (0b0101 & 0b0011 -> 0b0001)
7          | Bitwise OR          | \|    | 5 \| 3 -> 7 (0b0101 \| 0b0011 -> 0b0111)
8          | Bitwise XOR         | ^     | 5 ^ 3  -> 6 (0b0101 ^ 0b0011 -> 0b0110)
9          | Assignment          | =     | x = 5 + 3 -> 8

## Variables

Variable can store calculated value temporary.  
When you delete the expression of assignment, the variable value also delete.  
Constants, functions and keywords cannot be used variable names.  

```py
x = 10     # x:10
y = x ** 2 # y:100

PI = 3  # Error: 'PI' is a constant
pi = 3  # OK: It is case sensitive
```

## Functions

### Arithmetic

Identifier | Parameters  | Description                                                         | Examples
-----------|-------------|---------------------------------------------------------------------|--------------------
trunc      | (n)         | Returns the integer part of a number by removing any fractional     | trunc(1.5) -> 1, trunc(-1.5) -> -1
floor      | (n)         | Returns the largest integer less than or equal to a given number    | floor(1.5) -> 1, floor(-1.5) -> -2
ceil       | (n)         | Returns the number of rounded up to the next largest integer        |  ceil(1.5) -> 2,  ceil(-1.5) -> -1
round      | (n)         | Returns the value of a number rounded to the nearest integer        | round(1.5) -> 2, round(-1.5) -> -2
sin        | (n)         | Returns the sine of a number of degrees                             |
cos        | (n)         | Returns the cosine of a number of degrees                           |
tan        | (n)         | Returns the tangent of a number of degrees                          |
asin       | (n)         | Returns the arcsine (in degrees) of a number                        |
acos       | (n)         | Returns the arccosine (in degrees) of a number                      |
atan       | (n)         | Returns the arctangent (in degrees) of a number                     |
atan2      | (n1, n2)    | Returns the arctangent (in degrees) of a y (n2) and x (n1)          | atan2(1, 1) -> 45)
log10      | (n)         | Returns the base 10 logarithm of a number                           | log10(1000) -> 3
log2       | (n)         | Returns the base 2 logarithm of a number                            | log2(256) -> 8
log        | (n1, n2)    | Returns the natural logarithms or logarithm of a number (n1) with specified base (n2) | log(9, 3) -> 2)

### DateTime/TimeSpan

Identifier | Parameters  | Description                                          | Examples
-----------|-------------|------------------------------------------------------|---------------------------------
today      | ()          | Returns DateTime of the beginning of the current day | today() -> '2020/04/01 00:00:00'
now        | ()          | Returns DateTime of the current time                 | now() -> '2020/04/01 07:36:13'
dayofyear  | (datetime)  | Returns day of the year in a DateTime                | dayofyear('2020/04/01') -> '92'
dayofweek  | (datetime)  | Returns day of the week in a DateTime                | dayofweek('2020/04/01') -> 'wed'
daysinyear | (datetime)  | Returns how many days in specified year              | daysinyear('2020/04/01') -> 366
daysinmonth| (datetime)  | Returns how many days in specified month             | daysinmonth('2020/04/01') -> 30
wareki     | (datetime)  | Returns date of Japanese calendars, Kanshi (干支) and Rokuyo (六曜) | wareki('2020/04/01') -> '令和02年04月01日 庚子 大安'
kyureki    | (datetime)  | Returns date of old Japanese calendars, Kanshi (干支) and Rokuyo (六曜) | kyureki('2020/04/01') -> '令和02年03月09日 庚子 大安'
seconds    | (timespan)  | Returns total seconds of TimeSpan                    | seconds('+01:01:01') -> 3661
minutes    | (timespan)  | Returns total minutes of TimeSpan                    | minutes('+01:01:01') -> 61.016...
hours      | (timespan)  | Returns total hours of TimeSpan                      | hours('+01:01:01') -> 1.01694...
days       | (timespan)  | Returns total days of TimeSpan                       | days('+36:00:00') -> 1.5
weeks      | (timespan)  | Returns total weeks of TimeSpan                      | weeks('+3d 12:01:01') -> 0.5

## Constants

Identifier | Description
-----------|----------------------------------------------------------------------
PI         | The ratio of the circumference of a circle to its diameter (3.141...)
E          | The base of natural logarithms (2.718...)

## Reserved keywords

The following identifiers are reserved words and cannot be used as variable names.

```
if        elif      else      then      for
in        to        repeat    do        end
continue  break     exit      or        and
not
```

## License

MIT License
