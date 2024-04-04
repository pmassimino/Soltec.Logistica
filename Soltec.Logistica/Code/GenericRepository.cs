using Microsoft.EntityFrameworkCore;
using Soltec.Logistica.Data;

namespace Soltec.Logistica.Service
{
    public interface IGenericRepository<T> where T : class
    {
        public LogisticaContext context { get; set; }
        IQueryable<T> GetAll();
        T GetById(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(object id);
        void Save();
    }
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public LogisticaContext context { get; set; } = null;
        private DbSet<T> table = null;
        public GenericRepository()
        {
            this.context = new LogisticaContext();
            table = context.Set<T>();
        }
        public GenericRepository(LogisticaContext _context)
        {
            this.context = _context;
            table = _context.Set<T>();
        }
        public IQueryable<T> GetAll()
        {
            return table;
        }
        public T GetById(object id)
        {
            return table.Find(id);
        }
        public void Insert(T obj)
        {
            table.Add(obj);
        }
        public void Update(T obj)
        {
            table.Attach(obj);
            context.Entry(obj).State = EntityState.Modified;            
        }
        public void Delete(object id)
        {
            T existing = table.Find(id);
            table.Remove(existing);
        }
        public void Save()
        {
            context.SaveChanges();
        }
    }
}
