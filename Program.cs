using System;



abstract class BaseEntity
{
    public int Id { get; set; }
}

abstract class Product : BaseEntity
{
    private string _name;
    private decimal _price;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidProductException("Name boş ola bilməz");

            if (value.Length < 2)
                throw new InvalidProductException("Name ən azı 2 simvol olmalıdır");

            _name = value;
        }
    }

    public decimal Price
    {
        get => _price;
        set
        {
            if (value <= 0)
                throw new InvalidProductException("Price 0 və ya mənfi ola bilməz");

            if (value > 1_000_000)
                throw new InvalidProductException("Price həddindən artıq böyükdür");

            _price = value;
        }
    }

    // Bütün child class-lar özünə uyğun info çıxarsın
    public abstract void GetInfo();

    // Default implementation veririk (virtual)
    public virtual void CalculateDiscountedPrice(decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentException("Yanlış faiz");

        Price = Price - (Price * percent / 100);
    }
}


class Book : Product
{
    public string Author { get; set; }

    public override void GetInfo()
    {
        Console.WriteLine($"Id: {Id}, Name: {Name}, Price: {Price}, Author: {Author}");
    }
}

class Electronic : Product
{
    public string Brand { get; set; }

    public override void GetInfo()
    {
        Console.WriteLine($"Id: {Id}, Name: {Name}, Price: {Price}, Brand: {Brand}");
    }
}

class Clothing : Product
{
    public string Size { get; set; }

    public override void GetInfo()
    {
        Console.WriteLine($"Id: {Id}, Name: {Name}, Price: {Price}, Size: {Size}");
    }
}



class InvalidProductException : Exception
{
    public InvalidProductException(string message) : base(message) { }
}

class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message) : base(message) { }
}



static class IdGenerator
{
    private static int _id = 0;

    public static int GenerateId()
    {
        return ++_id;
    }
}



class Repository<T> where T : Product
{
    private T[] items = new T[0];

    public void Add(T item)
    {
        if (item == null)
            throw new ArgumentNullException("Item null ola bilməz");

        item.Id = IdGenerator.GenerateId();

        Array.Resize(ref items, items.Length + 1);
        items[^1] = item;
    }

    public T GetById(int id)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
                return item;
        }

        throw new ItemNotFoundException("Item tapılmadı");
    }

    public T[] GetAll()
    {
        return items;
    }

    public void Remove(int id)
    {
        int index = -1;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Id == id)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
            throw new ItemNotFoundException("Silinəcək item tapılmadı");

        for (int i = index; i < items.Length - 1; i++)
        {
            items[i] = items[i + 1];
        }

        Array.Resize(ref items, items.Length - 1);
    }
}



class Program
{
    static void Main()
    {
        try
        {
            Repository<Product> repo = new Repository<Product>();

            repo.Add(new Book
            {
                Name = "C# Basics",
                Price = 50,
                Author = "John Doe"
            });

            repo.Add(new Electronic
            {
                Name = "Laptop",
                Price = 1500,
                Brand = "Dell"
            });

            repo.Add(new Clothing
            {
                Name = "T-Shirt",
                Price = 25,
                Size = "M"
            });

            Console.WriteLine("---- ALL PRODUCTS ----");
            foreach (var item in repo.GetAll())
            {
                item.GetInfo();
            }

            Console.WriteLine("---- GET BY ID ----");
            var product = repo.GetById(2);
            product.GetInfo();

            Console.WriteLine("---- DISCOUNT ----");
            product.CalculateDiscountedPrice(10);
            product.GetInfo();

            Console.WriteLine("---- REMOVE ----");
            repo.Remove(1);

            foreach (var item in repo.GetAll())
            {
                item.GetInfo();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Xəta: {ex.Message}");
        }
    }
}
