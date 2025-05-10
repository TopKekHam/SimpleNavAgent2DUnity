
How to use:

* drag 2d collider into the scene
* drag NavMesh2D into a game object
* bake click bake
* drag NavAgent2D into a game object that you want to move on the mesh
* call from code the NavAgent2D Move method
* profit

Extra info

NavMesh2D:

* you need to bake the nav mesh each time you change any 2d collider
* the nav mesh are always centered to 0,0,0
* it does not support moving colliders

NavAgent2D

* you can assign the nav mesh you want to use, if not the nav agent will use the first nav mesh it finds

Caveats:

* the code is not optimized 
* code may contain bugs

Other

* read the public function on NavAgent2D and NavMesh2D to understand what you can do with it!
