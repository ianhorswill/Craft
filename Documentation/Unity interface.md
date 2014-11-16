Unity interface
===========

The Craft solver itself is implemented by the Craft.CSP class (CSP
stands for *Constraint Satisfaction Problem*).  Since creating a CSP
instance requires writing script code to create a CSP object and add
variables and constraints to it, we've added a wrapper that presents a
Craft CSP as a Unity component that can be edited in the editor.

Using the Randomizer class
-----------------------

The Unity interface is implemented by the Randomizer component, which
contains fields for its variables and constraints that can be edited
in the Unity inspector.

To use Craft in Unity, simple copy Craft.dll and Randomizer.cs into your project, then add a Randomizer component to whatever GameObject you like.  Having created a Randomizer, set the following fields as you like:

* **Variables**.  This is an array of objects giving the name, minimum, and maximum values for the variables you want to find values for.  These are the minimal set of fields you need to fill in.
    * If you want a given variable to be a vector, then fill in the minimum and maximum values for its Y and Z components in addition to the normal min and max fields (which are then treated as the min and max fields for the X component).
    * If you leave the Y and Z range zero, it assumes you want the variable to be a normal scalar variable
    * If you want the Randomizer to write the value of this variable to some other component, then fill in the Component and Property Name fields, and it will automatically update that property of that component when it finds a new value for the variable.
* **Constraints**.  This is an array of strings holding equations or inequalities (<= or >=) relating the allowable values of the variables to one another.  For example, you can say:
    * x = 2*y
    * x^2+y^2 = 1
    * x <= y
    * x+1 >= 2*y-7
* **Solve On Level Load**.  When this is true (i.e. the box is checked), the Randomizer will automatically find values for its variables, and write their values to other components, when appropriate, when its Start() method is called.  Otherwise, it does nothing until its Solve() method is called from a script.

Allowable constraints
------------------------
Constraints can be any of the following
* *numberExp* = *numberExp*
* *vectorExp* = *vectorExp*
* *numberExp* <= *numberExp*
* *numberExp* >= *numberExp*
* parallel(*vectorExp*, *vectorExp*)
* perpendicular(*vectorExp*, *vectorExp*)

Where *numberExp* is an expression for a number and *vectorExp* is an
expression for a vector.  Expressions can be any of the following:
* a number
* a variable
* *exp* + *exp*
* *exp* - *exp*
* *exp* * *exp*
* *exp* / *exp*
* *exp* ^ *postive_integer*
* (*exp*)
* function(*exp*)
* | *exp* |    (returns the magnitude of a vector)

The current set of functions is:
* sum(*exp1*, *exp2*, ... ,*expn*).  Returns *exp1*+... *expn*.
* average(*exp1*, *exp2*, ... ,*expn*), mean(*exp1*, *exp2*, ... ,*expn*).  Returns (*exp1*+...*expn*)/*n*.
* meanSquare(*exp1*, *exp2*, ... ,*expn*).  Returns the mean of the squares of all the *exp*s.
* meanSquareDifference(*offsetExp*, *exp1*, *exp2*, ... ,*expn*).  Returns the mean of *exp1*-*offsetExp*, *exp2*-*offsetExp*, ..., *expn*-*offsetExp*
* variance(*exp1*, *exp2*, ... ,*expn*).  Returns meanSquareDifference(average(*exp1*, *exp2*, ... ,*expn*), *exp1*, *exp2*, ... ,*expn*).

Performance tweaking
--------------------------

Craft uses a randomized binary search with chronological backtracking.
That means that if it makes a really bad choice early on, it can spend
an enormous amount of time exhaustively exploring a part of its search
space that just doesn't have any solutions to speak of.  So Craft
counts how many steps of the search it's tried and if it goes too long
without ever finding a solution, it gives up and starts over again
with a different set of random choices.  You can configure both how
many steps it will go before giving up and how many retries if will
attempt before giving up completely (and throwing an exception) with
the following fields:

* **Max Solver Steps**.  This is how long it will try to search before giving up and starting over.  If you have a lot of variables, this number should probably be increased.  On the other hand, if you just have a couple of variables, you can probably decrease it.  The default values as of this writing is 1000.
* **Max Restarts**.  This is how many times it will try to solve the problem before it gives up and throws an exception.  You probably won't need to mess with this value, although one can imagine problems where you might get better performance with a very small value for Max Solver Steps, and a large value for Max Restarts.  My recommendation is not to waste much time messing with this parameter, but you won't hurt yourself if you do.  The only bad thing that can happen is that if you make it too small, the system will give up and throw an exception when it doesn't need to.

Interface from script code
-------------------

The intention is that you be able to use the Randomizer without having
to write script code.  But if you want to access it from a script the
two members you need to know are:

* **Solve()**.  Forces the component to solve for a new set of numbers.
* **ScalarValue(string variableName)**.  Returns the value of the variable with the specified name, assuming it's a scalar (number) variable rather than a vector.
* **Vector3Value(string variableName)**.  Returns the value of the variable with the specified name, assuming it's a vector-valued variable.
* **ScalarValue(int variableIndex)**, **Vector3Value(int variableIndex)**, VariableIndex(string variableName).  Provide the same functionality, but using raw indicies into the Variables[] array rather than string names.  This saves doing a search through the variables array if you're going to do repeated lookups.
