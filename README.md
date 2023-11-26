# apilalang
## How to Use
```
apila {file} [-h | --help]
```

## Examples
### fib.apila
Prints the first 1000 elements of the fibonacci sequence
``` apila
0 0 1 

start: {
   dup print
   dup
   0 2 switch
   +
   0 2 switch
   1 +
   dup 1000 = if {
      goto end
   }
   0 2 switch
} goto start
end:
```

### fizzbuzz.apila
Classic fizzbuzz but following changes are made:  
- 11111 is printed instead of Fizz
- 22222 is printed instead of Buzz
- 33333 is printed instead of FizzBuzz  

Because strings are not supported in this language
``` apila
1

loop: {
   dup 100 = if {
      goto end
   }
   
   dup 3 % 0 = if {
      dup 5 % 0 = if {
         33333 print
      } else {
         11111 print
      }
   } else {
      dup 5 % 0 = if {
         22222 print
      } else {
         dup print
      }
   }

   1 +  
} goto loop
end:
```

### project euler problem 1.apila
The question: https://projecteuler.net/problem=1
``` apila
0 0

loop: 
   dup 1000 = if {
      goto exit
   }

   dup 3 % 0 = if {
      dup 
      1 2 switch
      +
      0 1 switch
   } else {
      dup 5 % 0 = if {
         dup
         1 2 switch
         +
         0 1 switch
      }
   }

   1 +
goto loop
exit:
drop print
```
## Keywords/Operators
- \+: Pops two elements, sums them up, pushes the result
- \-: Pops two elements, substracts them, pushes the result
- \*: Pops two elements, multiplies them, pushes the result
- /: Pops two elements, divides them, pushes the result
- %: Pops two elements, pushes the remainder
- print: Pops one element and outputs it to the console with a trailing new line character
- switch: Pops two elements, switches the order of corresponding elements in the stack. Popped elements are used as index. For example consider following program "10 20 30 40 0 2 switch print print print print" output is the following "20 30 40 10" which means stack was this "10 40 30 20" so it switched the 20 and 40. It is indexed from the top. 0 corresponds to 40 and 2 corresponds to 20.
- dup: Duplicates the top element in the stack
- goto: Jumps to the specified label. Label is specified AFTER the goto keyword
- sleep: Pops one element and sleeps that amount of milliseconds
- drop: Removes the top element in the stack
- if: Pops one element, compares the value of it to zero, does conditional jump if necessary. Scopes are determined with curly braces
- else: Sibling of if
- =: Checks if the top two elements in the stack are equal
- <: Checks if the second element is less than top element
- \>: Checks if the second element is greater than top element
- !: Negates the top element in the stack
## Notes
There are no data types only 64 bit floating point number so it is more a calculator than a programming language.  
Curly braces are necessary for "if" and "else". Other than that they can be used for grouping/scoping.

## References
Forth: https://en.wikipedia.org/wiki/Forth_(programming_language)  
Porth: https://gitlab.com/tsoding/porth  
Stack-oriented programming: https://en.wikipedia.org/wiki/Stack-oriented_programming  
Reverse Polish Notation: https://en.wikipedia.org/wiki/Reverse_Polish_notation  
Project Euler: https://projecteuler.net/