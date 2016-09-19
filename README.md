# SharedObjectToolkitClassLibrary

This project is a partial migration of a personnal class library fot high performance large object repositories and distributed, modularized software in .NET.

! WORK IN PROGRESS !

The parts of the library provided have many goals.

## Memory Manager

Provide a memory toolkit to allocate huge number of memory buffers (million or billion of small memory pointers) without Garbage Pauses problem. This memory is native one, accessed in unsafe mode : byte* not byte[]. GC pause is a common problem of every RAM based servers applications with large number of objects : when undreds million objects are created and Gen2 long live registered, the garbage collector struggle to collect memory, wich can cause minutes of pauses - not compatible with traditionnal SLA.

This lib provide :
* Fast memory allocator
* Smart pointers
* Native buffer manipulations
* Stream like buffer classes
* Fast Binary Serializer / Deserializer

## Large Object Repository

Based on a mapping of memory blocks and objects, this technic permit to virtually manipulate huge object collections. Each object had a pointer, and fields values are stored in a single block of memory, preserialized. For variable lenght value like strings, the block is expended or shrinked when the value changed. It include MVCC (Multi Value CoherencyControl), and object are immutable. For this reasons, there's no lock at all. Threads can access to object, read it and modify it in parallel, without any corrupted object state.

To store object in DB, a Key-Value store is based on ESENT build in Windows database. Serialization can be extremly fast, because objects are pre-serialized. But for practicality, object are serialized to be multi versionned : each version of object shema can be loaded, and updated to the latest.
* Huge number of objects in memory without GC pauses
* Immutable objects for lock free operation with MVCC
* Millions objects stores and retreived per seconds
* Back serialization in an ESENT store

## Data Structures

* Fast index stacks
* Fast index queues

