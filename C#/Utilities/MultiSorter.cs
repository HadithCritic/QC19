// https://www.techiedelight.com/sort-list-by-multiple-fields-csharp
// Sort List<Person> by name then age:
// [Fred,24]
// [Fred,30]
// [Roger,21]
// [Tom,27]

using System;
using System.Collections.Generic;

public class Person
{
    public string name;
    public int age;
    public Person(string name, int age)
    {
        this.name = name;
        this.age = age;
    }
    public override string ToString()
    {
        return "[" + name + "," + age + "]";
    }
}

/* Using LINQ
using System.Linq;

public class Program
{
    public static void Main()
    {
        Person tom27 = new Person("Tom", 27);
        Person roger21 = new Person("Roger", 21);
        Person fred24 = new Person("Fred", 24);
        Person fred30 = new Person("Fred", 30);

        List<Person> people = new List<Person>() { tom27, roger21, fred24, fred30 };

        List<Person> sorted = people.OrderBy(x => x.name)
                                    .ThenBy(x => x.age)
                                    .ToList();

        Console.WriteLine(String.Join(Environment.NewLine, sorted));
    }
}
*/

/* Using Sort(...)

public class Program
{
	public static void Main()
	{
		Person tom27 = new Person("Tom", 27);
		Person roger21 = new Person("Roger", 21);
		Person fred24 = new Person("Fred", 24);
		Person fred30 = new Person("Fred", 30);

		List<Person> people = new List<Person>() { tom27, roger21, fred24, fred30 };

		people.Sort((x, y) => {
						int ret = String.Compare(x.name, y.name);
						return ret != 0 ? ret : x.age.CompareTo(y.age);
					});

		Console.WriteLine(String.Join(Environment.NewLine, people));
	}
}
*/

/* Using IComparor on multi-column
public class PersonComparer : IComparer<Person>
{
	public int Compare(Person x, Person y)
	{
		if (object.ReferenceEquals(x, y))
			return 0;
		
		if (x == null)
			return -1;

		if (y == null)
			return 1;

		int ret = String.Compare(x.name, y.name);
		return ret != 0 ? ret : x.age.CompareTo(y.age);
	}
}
public class Program
{
	public static void Main()
	{
		Person tom27 = new Person("Tom", 27);
		Person roger21 = new Person("Roger", 21);
		Person fred24 = new Person("Fred", 24);
		Person fred30 = new Person("Fred", 30);

		List<Person> people = new List<Person>() { tom27, roger21, fred24, fred30 };

		people.Sort(new PersonComparer());
		Console.WriteLine(String.Join(Environment.NewLine, people));
	}
}
*/

/* Using Comparable on a single column
using System;
using System.Collections.Generic;

public class Person : IComparable<Person>
{
	public string name;
	public int age;

	public Person(string name, int age) {
		this.name = name;
		this.age = age;
	}

	public int CompareTo(Person emp)
	{
		if (emp == null)
			return 1;

		int ret = String.Compare(this.name, emp.name);
		return ret != 0 ? ret : this.age.CompareTo(emp.age);
	}

	public override string ToString()
	{
		return "[" + name + "," + age + "]";
	}
}
public class Example
{
	public static void Main()
	{
		Person tom27 = new Person("Tom", 27);
		Person roger21 = new Person("Roger", 21);
		Person fred24 = new Person("Fred", 24);
		Person fred30 = new Person("Fred", 30);

		List<Person> people = new List<Person>() { tom27, roger21, fred24, fred30 };

		people.Sort();
		Console.WriteLine(String.Join(Environment.NewLine, people));
	}
}
*/
