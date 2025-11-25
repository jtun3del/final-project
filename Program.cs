using NLog;
using System.Linq;
using NorthwindConsole.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger


var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();
Console.Clear();
logger.Info("Program started");

do


{
  Console.WriteLine("1) Display categories");
  Console.WriteLine("2) Add product");
  Console.WriteLine("3) Display Category and related products");
  Console.WriteLine("4) Display all Categories and their related products");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);
    if (choice == "1")
    {
        // display categories
        var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json");
        var config = configuration.Build();
        var db = new DataContext();
        var query = db.Products.OrderBy(p => p.ProductName);
        var discontinuedQuery = query.Where(p => p.Discontinued == true);
        var activeQuery = query.Where(p => p.Discontinued == false);
        Console.WriteLine("1 for active, 2 for discontinued, 3 for both");
    var selection = int.Parse(Console.ReadLine());
    if (selection == 1 || selection == 3)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"{activeQuery.Count()} records returned");
      Console.ForegroundColor = ConsoleColor.Magenta;
      foreach (var item in activeQuery)
      {
        Console.WriteLine($"{item.ProductName}");
      }
    }

    if (selection == 2 || selection == 3)
    {
      // display discontinued.
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"{discontinuedQuery.Count()} discontinued records returned");
      Console.ForegroundColor = ConsoleColor.Yellow;
      foreach (var item in discontinuedQuery)
      {
        Console.WriteLine($"{item.ProductName}");
      }
    }
    else
    {
      logger.Error("Not either 1, 2, or 3");
    }
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "2")
    {

    var db = new DataContext();
        // Add category
    Product product = new();
    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;
    Console.WriteLine("Enter the Supplier:");
    product.Supplier = GetSupplier(db);
    Console.WriteLine("Enter Category");
    product.Category = GetCategory(db);
    Console.WriteLine("Get quantity");
    product.QuantityPerUnit = Console.ReadLine();
    Console.WriteLine("Unit price");
    product.UnitPrice = int.Parse(Console.ReadLine());
    Console.WriteLine("units on order");
    product.UnitsOnOrder = (short?)int.Parse(Console.ReadLine());
    Console.WriteLine("reorderLevel");
    product.ReorderLevel = short.Parse(Console.ReadLine());
    Console.WriteLine("Discontinued (y/n)");
    product.Discontinued = Console.ReadLine() == "y" ? true : false; 



        ValidationContext context = new ValidationContext(product, null, null);

        List<ValidationResult> results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, context, results, true);
        if (isValid)
        {
      //var db = new DataContext();
      // check for unique name
      if (db.Products.Any(c => c.ProductName == product.ProductName))
      {
        // generate validation error
        isValid = false;
        results.Add(new ValidationResult("Name exists", ["ProductName"]));
      }
      else
      {
        logger.Info("Validation passed");
        // TODO: save category to db
        db.AddProduct(product);
            }
        }
        if (!isValid)
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }
        }
    }
    else if (choice == "3")
    {
        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryId);
        Console.WriteLine("Select the category whose products you want to display:");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
        }
        Console.ForegroundColor = ConsoleColor.White;
        int id = int.Parse(Console.ReadLine()!);
        Console.Clear();
        logger.Info($"CategoryId {id} selected");
        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
        Console.WriteLine($"{category.CategoryName} - {category.Description}");
        foreach (Product p in category.Products)
        {
            Console.WriteLine($"\t{p.ProductName}");
        }
    }
  else if (choice == "4")
  {
    var db = new DataContext();
    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName}");
      foreach (Product p in item.Products)
      {
        Console.WriteLine($"\t{p.ProductName}");
      }
    }
  }
    else if (String.IsNullOrEmpty(choice))
    {
        break;
    }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");

static Category? GetCategory(DataContext db)
{
  // display all blogs
  var categorie = db.Categories.OrderBy(b => b.CategoryId);
  foreach (Category c in categorie)
  {
    Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
  }
  if (int.TryParse(Console.ReadLine(), out int CategoryId))
  {
    Category category = db.Categories.FirstOrDefault(b => b.CategoryId == CategoryId)!;
    return category;
  }
  return null;
}

static Supplier? GetSupplier(DataContext db)
{
  // display all blogs
  var categorie = db.Suppliers.OrderBy(b => b.SupplierId);
  foreach (Supplier c in categorie)
  {
    Console.WriteLine($"{c.SupplierId}: {c.CompanyName}");
  }
  if (int.TryParse(Console.ReadLine(), out int SupplierId))
  {
    Supplier Supplier = db.Suppliers.FirstOrDefault(b => b.SupplierId == SupplierId)!;
    return Supplier;
  }
  return null;
}