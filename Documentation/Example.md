Craft (Constrained RAndom FloaTs) is a constrained random number
generator intended for PCG applications.  It generates sets of numbers
that satisfy declarative constraints specified by a designer.  It
includes a Unity interface that allows designers to create constrained
RNGs by entering:

* The names for the variables it should generate
* Their respective maximum ranges
* One or more equations or inequalities they should satisfy

Variables can also be tagged with a property of a Unity component
which should be set to the variable’s value.

For example, a simple build-point system for character creation can be
specified by creating a Randomizer component and entering into its
list of variables the desired names (e.g. str, const, dex, int, wis,
char, etc.), and specifying the desired range for each (e.g. 0-100,
although 20-100 might be more sensible).  The designer can then add to
its list of constraints the equation:

	str+const+dex+int+wis+char = 300 

This says, you don’t care what the individual attributes are so long
as they sum to 300 build points.  If you want to make different
attributes cost different numbers of build point, you simply multiply
each by its cost, e.g.:

	1.5*str+const+dex+1.2*int+wis+char = 300 

You can add other constraints too.  If, for example, you want to
enforce that the character is brawny rather than brainy, just add the
constraint:

	str >= int+20

Which states that the strength has to be at least 20 points higher
than the intelligence.  Similarly, you might want to enforce that the
strength and constitution be somehow comparable, so you don’t have a
tremendously strong character with the constitution of a 90 year old
tuberculosis patient.  For that, you can just say something like:

	const >= str*0.8

(i.e. constitution must be at least 80% of strength) and/or:

	str >= const * 0.8

(The opposite)

This all works, but it’s still happy to generate solutions where two
attributes are near the maximum value and the rest are near the
minimum.  You can force a more balanced character by adding the
constraint that the variance of the attributes be no larger than some
value (the variance is roughly the square of the average spread of the
numbers):

	variance(str, const, dex, int, wis, char) <= 150

Caveats
——
* This is research software.  It’s not as polished or optimized as a
professional product.  It’s also essentially in alpha test.  That
said, I’m motivated to have it used in the real world, so I’m happy to
help out with tech support and feature requests, where possible. 
* Craft is fast by the standards of constraint solvers, but it’s still
very slow compared to a normal RNG.  Here are some example timings: 
   * Find two perpendicular 3-vectors: 8usec (average)
   * Pick a random unit vector: 11usec
   * Find a solution to 10 <= a^2 +b <= 20: 7 usec
* Craft uses a smart, randomized binary search algorithm to find
solutions to the constraints, and search algorithms are exponential in
the worst case.  So while simple examples like the one above run
fairly efficiently, things can get slow fast as you make things more
complicated. 
* Craft does not sample uniformly in most cases, although it doesn't
go out of its way to be biased.  But if you need uniform sampling,
you'll need to use a different technique.
