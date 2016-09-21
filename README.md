# SharedObjectToolkitClassLibrary for .NET

Imagine you can load, access, modify and asynchronously save to disk billions of immediatly accessible objects using concurent thread that are communicates at nanosecond latency delay, without any locks nor GC pauses ?

This is the goal of this project : built heavy single process in memory mlti-threaded high performance applications. This project is a partial migration of a personnal class library.

! WORK IN PROGRESS !

The parts of the library provided have several components.

## Memory Manager

Memory toolkit to allocate huge number of buffers (millions or billions of memory blocks) without Garbage Pauses problem. This memory is native one, allocated on the processus heap, accessed in unsafe mode : byte* not byte[]. GC pauses is a common problem of every RAM based servers applications with large object collections : when undreds million objects are created and Gen2 long live registered, the garbage collector struggle to collect memory, wich can cause long of pauses (seconds on munites) not compatible with traditionnal SLA.

This lib provide :
* Fast memory allocator, based on sub partitionning of memory "pages".
* Smart pointers, able to manage the release of blocks and being immutable.
* Native buffer manipulations : copy, move, insert, etc.
* Stream and array like buffer classes
* Custom Binary Serializer / Deserializer, faster than .net one because endianness agnostic.

## Large Object Repository

The technic at the heart of this repository are named Virtual Objects. Based on a mapping of memory blocks and objects, this object repository permit to virtually manipulate huge object collections. Each object had a pointer, and fields values are stored in a single block of memory, preserialized. For variable lenght value like strings, the block is expended or shrinked when the value changed. This objects are immutable. For this reasons, there's no lock at all. Threads can access to object, read it and modify it in parallel, without any corrupted object state. When you get an object from the repository, he will never change, and you can change it and submit it to the repository to be the new version.

To store object in DB, a Key-Value store is based on ESENT build-in Windows database. Serialization can be extremly fast, because objects are pre-serialized. But for practicality, each field are serialized separatly to be multi versionned : each version of object shema can be loaded, and updated to the latest.

* Huge number of objects in memory without GC pauses
* Immutable objects for lock free operation with MVCC
* Millions objects stores and retreived per seconds
* Back serialization in an ESENT store, asynchronously
* No transaction concept, but isolation permit to decide to submit changes in groups

## Data Structures

* Fast index stacks
* Fast index queues

