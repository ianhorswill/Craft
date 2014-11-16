How it works
=========

When you set up a problem in Craft, you specify a set of variables and
*bounds* on those variables.  You're asking Craft to randomly choose
values for each variable within its respective bounds, such that all
the values satisfy the constraints.

So we can think of the problem algebraically as a bunch of variables
and some constraints.  However, we can also think of it geometrically.
If you have *n* variables (assume they're floats not vectors), then
they collectively define an *n*-dimensional space and the constraints
define some region inside that space.  Moreover, the bounds on all the
variables collectively define a *bounding box* in that space.  We're
searching for a point within that bounding box that's also inside the
magic region defined by the constraints, which is typically called the
feasible region, but we'll called the *solution region* for clarity.

Rejection sampling
----------------

The most obvious way to solve the problem would be to repeatedly guess
points in the bounding box and test them against the constraints until
we happened to choose one that worked.  This is called *rejection
sampling*.  It has the advantage of being simple, but the disadvantage
requiring on average a number of tries proportional to the ratio of
size of the bounding box to the size of the solution region.  That is,
as the solution region get smaller, the amount of work you need to do
goes up fast.  So given variables x and y both in the range between +1
and -1, finding a point in the unit circle would be fast because
roughly 3/4 of the points in the bounding box are solution points.
However, finding a point *on* the unit circle using double-precision
floating point would require around 10^19 tries on average, or around
317 years, even if you could pick and test a point every nanosecond.

Testing bounding boxes
------------------------

The basic trick used by Craft is to find tight bounding boxes from
loose bounding boxes using interval arithmetic (see below).  That is,
given a bounding box, it can find the smallest bounding box that still
contains all the solution points of the original bbox.

Tightening does two things for us.  First, it lets us reduce the space
we're searching, which is always a help.  It also lets us test an
entire bbox for the existence of solutions: if we tighten the bbox and
get back an bbox, then the original bbox didn't contain any solutions.

Binary search
-------------

This lets us do binary search: we start with our original bbox, split
it in two, choose one half, tighten it, and check if the tightened
bbox is empty.  If so, we try the other half instead.  But if it's
non-empty, we split it again, continuing until we've split it down to
the level of our floating-point resolution, i.e. where it contains
only one floating-point number.

We split the bbox by picking a variable, whose value we already know
lies in some interval and splitting the interval in half, leaving the
other variables along.  So if before the split, the variable's
interval was [a, b], i.e. its value had to be between the numbers a
and b, then after the split it will be either [a, (a+b)/2] or
[(a+b)/2, b].  By randomizing our splitting decisions (both which
axis/variable we split on and which half we choose), we randomize the
point we get as a solution.

Optimistic binary search
--------------

Hopefully, you became somewhat uneasy at the phrase "split it down to
the level of our floating-point resolution," since that is rather a
lot of splitting.

Fortunately, we can make the system much more efficient.  Instead of
picking a variable and splitting its interval in half, we pick a
variable and guess a random value for it.  That is, if the variable
was previously in the range [a, b], we randomly choose some value v in
that range and narrow the variable to the range [v, v], so that it
only has one possible value.  Then we test whether the resulting
bounding box still has any solutions.  If so, we proceed, otherwise we
give up on the value v and instead split the variable's interval as
above.

That's the basic algorithm.  The rest is just a matter of efficiently shrinking the bounding boxes and undoing things when we need to backtrack.

Example
--------

Suppose we give the system a problem with two variables, x and y, both
in the range [-1,1], and the constraint x^2 + y^2 =1.  A typical run
of Craft's solver will look like this:

* Pick a variable to narrow.  Let's say it's x.
* Pick a random value for it in its current range.  Let's say it chooses 0.1.  So now x is no longer in the range [-1,1], it's in the range [0.1, 0.1], i.e. it only has one possible value.
* We solve for the possible values for y given that x=0.1.  If we did this with a fancy algebra system like Mathematica, it would get that y^2 = 0.99 and so y = +/-0.995.  However, Craft can only represent variable values as intervals (bounding boxes), so it updates y to be in the interval [-0.995, +0.995].  So it loses the information that it can really only have two possible values.
* At this point, the algorithm needs to narrow another variable, and the only one left to narrow is y.  So it randomly picks a value in y's range [-0.995, +0.995].  Let's assume it chooses -0.3.
* Next, it checks x=0.1, y=-0.3 against the constraint equation, which fails.
* So it gives up on assuming y=-0.3, and instead splits y's interval [-0.995, +0.995] into two subintervals: [-0.995, 0], and [0, 0.995].  It randomly picks one.  Let's assume it picks the positive one.
* Now it has a bounding box that it's searching where x is in [0.1, 0.1] and y is in the range [0, 0.995].  It tightens that bounding box and gets that y's real range is [0.995, 0.995], i.e. it can have only one value.
* And so it's done.

Computing tight bounding boxes
----------------

The tightening operation uses interval arithmetic, i.e. the basic
arithmetic operations extended to intervals (ranges).  So [1,2]+[3,5]
= [4, 7], i.e. if you add a number in the range 1-2 to a number in the
range 3-5, you're going to get a number in the range 4-7.  You can
extend this basic idea to all arithmetic operations, although division
turns out to be painful to deal with (this is ultimately why Craft
forgets that y can only have one of two values in the example above).
You can also extend it to improper intervals where the high and/or low
bounds are infinite.  Again, this is straightforward except that
division is a pain (you end up with a 14-way case analysis).  See the
Wikipedia entry on interval arithmetic for more information and/or
contact me for pointers to the relevant literature.

Once you've implemented interval arithmetic, tightening bounding boxes
is straightforward.  For each constraint, you resolve for the range of
each variable in the constraint given the current ranges of the other
variables.  If you get a narrower range than the variable previously
had, you narrow it.

For example, assume you have the constraint x+y=0, with x in the range
[-1,1] and y in the range [-2,2]..  We would start by seeing if we can
narrow those ranges at all (i.e. tighten the bounding box.  So we
solve for x:

x = [0,0] - [-2, 2] = [-2, 2]

To learn that x has to be in the range [-2, 2].  But since we already
had limited it to [-1,1], that doesn't change anything.  On the other
hand, when we solve for y:

y = [0,0] - [-1, 1] = [-1, 1]

that's a narrower range than we had previous had for y.  So we can update it.

So far so good.  Now suppose the algorithm decides to work on x.  It
starts by picking a random value for it.  So suppose it picks 0.5.
Then we update x=[0.5,0.5] and recompute y:

y = [0, 0] - [0.5, 0.5] = [-0.5, -0.5]

Which is a narrower range than y had previously had, and moreover is a
single value.  So we've now found values for both variables and we're
done.