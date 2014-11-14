This is a floating-point constraint solver intended for use in games,
particularly for procedural-content generation.  It includes a Unity
wrapper to make it easy to use within the Unity editor.

The Craft/ subfolder contains the VS project for the solver itself.
Build it once and drop the DLL into your Unity project.

The Unity/ subfolder contains the sources for the Unity wrapper,
Randomizer.cs.  It allows you to enter a set of variables to solver
for in the editor, along with a set of constraint equations to limit
their values.  It will automatically solve for the values of the
variables and optionally store them back to specified fields of other
Unity components at level load time.

The Unity/ directory also contains two testing components, RandomizerTester
which just runs a Randomizer over and over again and logs the results
to a .csv file you can look at in Excel, and RandomizerVisualizer which
does roughly the same thing, but displays the results as a particle system.

Caveats
=======
* This is research software, not a product, so it's not as extensively
tested as a commercial product.
* Any constraint solver is ultimately a search algorithm, including this
one.  That means it has exponential complexity in the worst case.  So while
it can run very fast, it can also slow down fast as you add more constraints
to solve.  All the Unity components display performance information, so you
should at least shouldn't be in for any surprises.
* It makes no guarantees about the uniformity of the probability distribution.
It will only be uniform in very simple cases.  In others, it will be non-uniform
but hopefully not too bad.  You can use the Visualizer to get a good sense of
what a given constraint system will give you.
* The constraint propagation algorithms assume a given variable only occurs
once in a given constraint equation.  It should still find valid results if
in other cases, but it will do more search than it needs to.  So if you want
to square a variable, always say x^2 rather than x*x.
