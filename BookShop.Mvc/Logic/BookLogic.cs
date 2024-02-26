using System;
using System.Linq;
using BookShop.Mvc.Models;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Mvc.Logic
{
    public interface IBookLogic
    {
        Book GetBookById(int id);
        Book[] Search(string searchTerm);
        Book[] FindCheapBooks(int numberOfBooks);

        byte Add(Book book);
        byte Delete(Book book);
        byte Edit(Book book);
    }

    public class BookLogic : IBookLogic
    {
        private readonly IDatabaseContext _databaseContext;
        const bool InsertBook = true;
        const bool UpdateDeleteBook = false;

        public BookLogic(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Book GetBookById(int id)
        {
            return _databaseContext.Books.First(b => b.Id == id);
        }

        public Book[] Search(string searchTerm)
        {
            var terms = searchTerm?.Split(' ') ?? new string[0];
            var predicate = terms.Aggregate(
                PredicateBuilder.New<Book>(string.IsNullOrEmpty(searchTerm)),
                (acc, term) => acc.Or(b => b.Title.Contains(term, StringComparison.InvariantCultureIgnoreCase))
                    .Or(b => b.Author.Contains(term, StringComparison.InvariantCultureIgnoreCase)));

            var books = _databaseContext.Books.AsExpandable()
                .Where(predicate)
                .OrderBy(b => b.Title)
                .ToArray();
            return books;
        }

        public Book[] FindCheapBooks(int numberOfBooks)
        {
            var cheapBooks = _databaseContext.Books.OrderBy(b => b.Price)
                .Take(numberOfBooks)
                .OrderBy(b => b.Title)
                .ToArray();
            return cheapBooks;
        }

        private byte InsertEditValidate(Book book, bool insert)
        {
            if (book.Id == 0)
                return 1;           // Id is rquired.
            
            if (insert) {
                var foundBook = _databaseContext.Books.Find(book.Id);
                if (foundBook != null)
                    return 2;           // Id is duplicated.
            }

            if (string.IsNullOrEmpty(book.Title))
                return 3;           // Title is rquired.

            if (string.IsNullOrEmpty(book.Author))
                return 4;           // Author is rquired.

            if (book.Price < 0.01m | book.Price > 10000.0m)
                return 7;            // Out of range for Price.

            if (book.Quantity < 1 | book.Quantity > 100)
                return 8;            // Out of range for Quantity.
            return 0;
        }

        private byte DeleteValidate(Book book)
        {
            if (book.Id == 0)
                return 1;           // Id is rquired.

            Book foundBook = _databaseContext.Books.Find(book.Id);
            if (foundBook == null)
                return 9;   //book not found;

            OrderLine orderlineFound = _databaseContext.OrderLines.SingleOrDefault(ol => ol.Book.Id  == book.Id);
            if (orderlineFound != null)
                    return 5;           // Book's Id is used in Order

                return 0;
        }
        public byte Add(Book book)
        {
            byte ValidationResult = InsertEditValidate(book,InsertBook);
            if (ValidationResult != 0)
                return ValidationResult;

            _databaseContext.Books.Add(book);
            return 0;   //book created successfully
        }

        public byte Delete(Book book)
        {
            Book foundBook = _databaseContext.Books.Find(book.Id); //SingleOrDefault(b => b.Id == book.Id);
            if (foundBook == null)
                return 9;   //book not found;

            byte ValidationResult = DeleteValidate(book);
            if (ValidationResult != 0)
                return ValidationResult;
            _databaseContext.Books.Remove(foundBook);
            _databaseContext.SaveChanges();
            return 0;   //book deleted successfully
        }

        public byte Edit(Book book)
        {
            Book foundBook = _databaseContext.Books.Find(book.Id);// SingleOrDefault(b => b.Id == book.Id);
            if (foundBook == null)
                return 9;   //book not found;
            byte ValidationResult = InsertEditValidate(book,UpdateDeleteBook);
            if (ValidationResult != 0)
                return ValidationResult;
            foundBook.Title = book.Title;
            foundBook.Author = book.Author;
            foundBook.Price = book.Price;
            foundBook.Quantity = book.Quantity;
            foundBook.Price = book.Price;
            _databaseContext.SaveChanges();
            return 0; //book edited successfully
        }
    }
}
