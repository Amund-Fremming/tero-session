# Tasklist

**First**

New cache

- generic
- take in lambda with game object, and have return data so i can broadcast in my hubs
- serialization in controllers
- use locks
- bg service to remove from the connection id service, game service and from hubs

- Add time to live for session cache so it does not fill up
  - if game finished, remove it from the cache manuallt also
- Create cache for connection id => (ttl, game_key, user_id)
  - Bg task for eviction
  - used when failed diconnect
  - bg task gets cleanup funciton from constructor
  - simple methods for gettin, removing and settting
- Add semaphores slim for thread safe updates on game session
- Add map in hub for mapping connection id to user id and add funciton for adding in this, also needs ttl, and eviction
- Disconnect funcitonality for spin game
- add userid validation for host id in spin game
- Connect to rust backend to be able to post log and free key
- System log builder?
- Add disconnect logic and retries, or do this from frontend
- HostId and validation for start quiz game

**Later**

- Make SpinGame generic and only possible to inherit from
  - then implement a game mode named roulette
  - then i can implement a Spinner game
  - then i can imlement some other thingy
