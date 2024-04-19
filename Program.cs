//Створення простого застосунку для керування замовленнями
// Ваша програма повинна містити наступні елементи:
// 1.Створення інтерфейсу IOrder, який містить методи для додавання товарів, видалення товарів та отримання загальної вартості замовлення.
// 2.Створення класу Order, який реалізує інтерфейс IOrder та містить методи для роботи з замовленнями.
// 3.Побудова ієрархії класів для товарів: базовий клас Product, який містить загальні властивості, та похідні класи, наприклад, FoodProduct, ElectronicProduct тощо.
// 4.Використання конструкторів для ініціалізації об'єктів класів та деструкторів для звільнення ресурсів.
// 5.Визначення події для сповіщення про зміну статусу замовлення та організація взаємодії об'єктів через цю подію.
// 6.Реалізація узагальненого класу для зберігання списку товарів у замовленні.
// 7.Створення класів винятків для обробки помилок під час роботи з замовленнями

using System;
using System.Collections.Generic;
using System.IO;

// 1. Інтерфейс для замовлення
interface IOrder
{
    void AddItem(Product product);
    void RemoveItem(Product product);
    decimal GetTotalCost();
}

// 3. Базовий клас для товарів
abstract class Product : IDisposable
{
    public string Name { get; set; }
    public decimal Price { get; set; }

    private bool disposedValue; // Для відстеження стану об'єкта
    private StreamWriter logWriter; // Приклад некерованого ресурсу

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
        logWriter = new StreamWriter($"{Name}.log"); 
    }

    ~Product()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                logWriter.Dispose();
            }

            
            Console.WriteLine($"Destructor called for {Name}. Releasing unmanaged resources.");
            logWriter.Dispose(); 

            disposedValue = true;
        }
    }
}

// 3. Похідний клас для продуктів харчування
class FoodProduct : Product
{
    public DateTime ExpirationDate { get; set; }

    public FoodProduct(string name, decimal price, DateTime expirationDate) : base(name, price)
    {
        ExpirationDate = expirationDate;
    }
}

// 3. Похідний клас для електронних товарів
class ElectronicProduct : Product
{
    public int WarrantyMonths { get; set; }

    public ElectronicProduct(string name, decimal price, int warrantyMonths) : base(name, price)
    {
        WarrantyMonths = warrantyMonths;
    }
}

// 2. Клас для замовлення
class Order : IOrder
{
    private List<Product> items = new List<Product>();

    public event EventHandler? OrderStatusChanged;

    public void AddItem(Product product)
    {
        items.Add(product);
        OnOrderStatusChanged(new EventArgs());
    }

    public void RemoveItem(Product product)
    {
        items.Remove(product);
        OnOrderStatusChanged(new EventArgs());
    }

    public decimal GetTotalCost()
    {
        decimal total = 0;
        foreach (var item in items)
        {
            total += item.Price;
        }
        return total;
    }

    protected virtual void OnOrderStatusChanged(EventArgs e)
    {
        OrderStatusChanged?.Invoke(this, e);
    }
}

// 6. Клас для списку товарів у замовленні
class OrderItemList<T> : List<T> where T : Product
{
    public decimal GetTotalCost()
    {
        decimal total = 0;
        foreach (var item in this)
        {
            total += item.Price;
        }
        return total;
    }
}

// 7. Клас для винятку під час роботи з замовленнями
class OrderException : Exception
{
    public OrderException(string message) : base(message) { }
}

// Клас програми
class Program
{
    static void Main(string[] args)
    {
        Order order = new Order();
        order.OrderStatusChanged += Order_OrderStatusChanged;

        FoodProduct apple = new FoodProduct("Apple", 0.5m, DateTime.Now.AddDays(7));
        ElectronicProduct laptop = new ElectronicProduct("Laptop", 1000m, 12);

        order.AddItem(apple);
        order.AddItem(laptop);

        Console.WriteLine($"Total cost: {order.GetTotalCost()}");

        order.RemoveItem(apple);

        Console.WriteLine($"Total cost: {order.GetTotalCost()}");

        OrderItemList<Product> orderItems = new OrderItemList<Product>
        {
            laptop,
            new FoodProduct("Banana", 0.3m, DateTime.Now.AddDays(5))
        };

        Console.WriteLine($"Total cost of order items: {orderItems.GetTotalCost()}");

        // Звільнення ресурсів
        apple.Dispose();
        laptop.Dispose();
    }

    // 5. Метод для обробки події зміни статусу замовлення
    private static void Order_OrderStatusChanged(object sender, EventArgs e)
    {
        if (sender is Order order)
        {
            Console.WriteLine("Order status changed. Total cost: " + order.GetTotalCost());
        }
    }
}