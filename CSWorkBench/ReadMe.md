# CSWorkBench

An UI-App which

Hosted and rendered from server

## Concept

connected to one Database per one Registry (like Memory)

maybe blocking objects per registry could be done to avoid conflicting actions from diffrent users (use singleton / scoped services)

### Routing

web-assembly-server-host/ -> search Tab

web-assembly-server-host/{ObjectId:int}/ -> display the Object

web-assembly-server-host/{ObjectId:int}/\*\*{some Pathing}/ -> display object routed to, navigation uses such Paths

### Contents

Main: Dynamic display of Object with Attributes and Methods

some buttons for new, search, settings

Opt. Right: AI-Assistant

Opt. Somewhere: settings (user management)

### Actions

creating objects

Editing objects (blocking)

run Functions (blocking)

deleting objects

on starting action (except create) check if nothing changed in registry

### Drag and Drop / multi object usage

**Goal** 

allow the user to have multiple objects opened, and move Data between them.

**Approaches**

1. allow multiple Windows within one Browser
   - difficult to manage url's
   - easy to handle drag and drop within one tab (ScopedServies)
2. allow multiple Tabs with one Object each
   - difficult to manage Drag and Drop
   - difficult for user to manage browser tabs
   - easy to manage object visualization

**Note**
when implementing cross browser dragging, this can be used within tabs as well

**RoadMap**

1. User identification per browser localstorage
2. implement cross Tab dragging
3. maybe implement workspace within a tab

### Agenting

I think agent should be seperate from UI (maybe custom Design)

Just handled as a normal object, where you can drag context to (Context maybe as Attachements -> would create dependency on DynStore)

## UI

### Routing

always route to Object View

root:int optional

sub:int optional

invalid -> like not found

### Object View

Not Found -> Object Search

root? -> get object from DB
no root -> New Object

sub? -> get sub-object from RootObject

object 
   -> Get Typeinfo from reflectionService
   -> optional feature: get Settings Global/user
   -> render

### Object Search

TODO

### New Object

Action: New -> create and route to empty entry in DB

Type empty? -> provide Type Selection

Object empty? -> provide Constructors (function), type still changeable

object created -> dont render new object component (auto)