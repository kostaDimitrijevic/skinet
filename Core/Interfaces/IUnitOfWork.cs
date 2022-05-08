using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IUnitOfWork : IDisposable //IDisposable ce da trazi disposed metodu u nasoj UnitOfWork klasi i kada zavrsimo transkaciju izbrisace nas Context 
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        Task<int> Complete(); // vraca broj promena u bazi
    }
}