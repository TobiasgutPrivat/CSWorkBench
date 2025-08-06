# Concept

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
