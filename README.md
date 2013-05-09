PervasiveSensorNetwork
======================

Description
-----------

A Sensor network using Arduinos as Coordinator and Remotes. (Arduinos, Redis Pub/Sub, C#)

There are 4 major layers in the project:

- The Physical layer, represented by the Arduino network, and retreiving physical values ( here luminosity and Temperature over a week).
- The Process layer, represented by a C# program that listens to the Coordinator serial port.
- The Data Layer, using Redis Database. Redis is used to Left push arduino records on a given key, representing an arduino node.
Redis pushes new entries to Tier-Applications using Pub/Sub.
- The Tier Layer will subscribe to Redis updates on the following channel: ERROR, TEMPERATURE, LUMINOSITY.


Tools
-----

: Redis.Io : http://redis.io/
: X-CTU Xbee firmware configuration: http://www.digi.com/support/productdetail?pid=3352&osvid=57&type=utilities
: Arduino: http://www.arduino.cc/

