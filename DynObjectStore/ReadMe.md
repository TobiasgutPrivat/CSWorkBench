# Setup

**Requires** 
- some DB server with according implementation of IDBConnection and according schema
- initialization of DBConnection and Registry

## Tested:

PgSqlServer with schema from init/schema.sql ([download](https://www.postgresql.org/download/))

PgAdmin4 and Command Line Tools are usefull for administration and testing

## Untested

docker-compose maybe works

# Concept

based on Idea of a (Global entity-centric IT Environement)[https://www.notion.so/Global-Entity-Centric-IT-1bd37f05f33a8024bd0aea28f3f3baba]

Objects:

- model: (id, class, serialized data)
- instances of specified class
- serialization per jsonserializer (recreates)

Attachments:

- model: (ParentId, path, name, ObjectId)
- represents the conection of an object attached to another object
- location stored as ObjectId + path within object + Attrname (e.g. #1234, somelist.2 , "an_attachment")

DB:

- models stored as tables, maybe include vector for objects

RealTime-Memory:

- registry for ObjectIds: {Object: ObjectId}
- registry for Objects: {ObjectId: Object}
- registry for attachements {<Object, path, name>, Object} used to track what object is at a specific path
- registry for attachementIds {Object, [<Object, path, name>]} used to track what paths are there for one parent

## Decisions

- implementing git like feature in here, does not make sense because this is about local computing, git like systems can though be implemented as C# module allowing interaction from Computer to server or similair

## issues

- recursion does not work if classes don't have parameterless constructors
