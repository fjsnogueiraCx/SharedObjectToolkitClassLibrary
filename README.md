# SharedObjectToolkitClassLibrary

This project is a partial migration of a personnal libs fot high performance large object repositories and distributed, modularized software in .NET.
The parts of the library provided have many goals.

## Memory Manager

Provide a memory toolkit to allocate huge number of memory buffers (million or billion of small memory pointers) without Garbage Pauses problem. This memory is notive one, accessed in unsafe mode. It is a common problem of every RAM based servers applications with large number of objects : when undreds million objects are created and Gen2 long live registered, the garbage collector struggle to collect memory, wich can cause minutes of pauses - not ompatible with traditionnal SLA.

This lib provide :
* Fast memory allocator
* Smart pointers
* Native buffer manipulations
* Stream like buffer classes
* Fast Binary Serializer / Deserializer

## Data Structures

* Fast index stacks
* Fast index queues

## Large Object Repository

Based on a mapping of memory blocks and objects, this technic permit to virtually manipulate huge object collections :
