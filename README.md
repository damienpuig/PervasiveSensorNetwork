PervasiveSensorNetwork
======================
This project has been developed for the "Pervasive Application" module, P00602, Oxford Brookes University.


Description
-----------

A Sensor network using Arduinos as Coordinator and Remotes. (Arduinos, Redis Pub/Sub, C#)

There are 4 major layers in the project:

- The Physical layer, as the Arduino network, retreiving physical values (Luminosity and Temperature for a week).
- The Process layer, represented by a C# program that listens to the Coordinator serial.
- The Data Layer, using Redis Database. Redis is used to Left push arduino records on a given key (e.g. arduino node 1). Redis pushes new entries to Tier-Applications using Pub/Sub.
- The Tier Layer will subscribe to Redis updates on the following channel: ERROR, TEMPERATURE, LUMINOSITY.


Tools
-----

- Redis.Io : http://redis.io/
- X-CTU Xbee firmware configuration: http://www.digi.com/support/productdetail?pid=3352&osvid=57&type=utilities
- Arduino: http://www.arduino.cc/

