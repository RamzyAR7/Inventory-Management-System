﻿using IMS.Application.DTOs.Supplier;
using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.Application.Services.Interface
{
    public interface ISupplierService
    {
        Task<(IEnumerable<Supplier> Items, int TotalCount)> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Supplier, bool>> predicate = null,
            Expression<Func<Supplier, object>> orderBy = null,
            bool sortDescending = false,
            params Expression<Func<Supplier, object>>[] includeProperties);
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier> GetByIdAsync(Guid id);
        Task CreateAsync(SupplierReqDto supplierDto);
        Task UpdateAsync(Guid id, SupplierReqDto supplierDto);
        Task DeleteAsync(Guid id);
    }
}
