# Tasklist

**First**

- make cache and manager take in ttl

- Cahce improvement: Not so many upsert functions but also not so verbose return data

- Add time to live for session cache so it does not fill up

  - if game finished, remove it from the cache manuallt also

- Bg task for eviction

  - used when failed diconnect
  - bg task gets cleanup funciton from constructor
  - simple methods for gettin, removing and settting

- Connect to rust backend to be able to post log and free key

- HostId and validation for start quiz game with new host logic

- Unit tests for caches, and bg cleanup? Just ai generate it

**Later**

- Make SpinGame generic and only possible to inherit from
  - then implement a game mode named roulette
  - then i can implement a Spinner game
  - then i can imlement some other thingy
