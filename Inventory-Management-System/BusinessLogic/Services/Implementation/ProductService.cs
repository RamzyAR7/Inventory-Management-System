using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Products;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _unitOfWork.Products.GetAllAsyncWithNestedIncludes();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Products.GetAsyncWithNestedIncludesBy(e => e.ProductID == id);
        }

        public async Task CreateAsync(ProductReqDto productDto)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(e => e.ProductName == productDto.ProductName);
            if (existingProduct != null)
            {
                throw new Exception("Product already exists");
            }
            var product = _mapper.Map<Product>(productDto);
            product.ProductID = Guid.NewGuid();
            await _unitOfWork.Products.AddAsync(product);

            foreach (var supplierId in productDto.SuppliersIDs)
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(e => e.SupplierID == supplierId);
                if (supplier != null)
                {
                   await _unitOfWork.SupplierProducts.AddAsync(new SupplierProduct
                         {
                            SupplierID = supplier.SupplierID,
                            ProductID = product.ProductID
                         });
                }
            }
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(Guid id, ProductReqDto productDto)
        {
            var existingProduct = await _unitOfWork.Products.GetAsyncWithNestedIncludesBy(e => e.ProductID == id);
            if (existingProduct == null)
            {
                throw new Exception("Product not found");
            }

            _mapper.Map(productDto, existingProduct);
            await _unitOfWork.Products.UpdateAsync(existingProduct);

            // Get current supplier IDs
            var currentSupplierIds = existingProduct.Suppliers.Select(sp => sp.SupplierID).ToList();

            // Get new supplier IDs from the form
            var newSupplierIds = productDto.SuppliersIDs ?? new List<Guid>();

            // 1. Remove unselected suppliers
            var suppliersToRemove = existingProduct.Suppliers
                .Where(sp => !newSupplierIds.Contains(sp.SupplierID))
                .ToList();

            foreach (var supplierToRemove in suppliersToRemove)
            {
                var supplierProduct = await _unitOfWork.SupplierProducts.FirstOrDefaultAsync(
                    sp => sp.SupplierID == supplierToRemove.SupplierID && sp.ProductID == existingProduct.ProductID);

                if (supplierProduct != null)
                {
                    await _unitOfWork.SupplierProducts.DeleteAsync(supplierProduct.SupplierID);
                }
            }

            // 2. Add new suppliers (not already existing)
            foreach (var supplierId in newSupplierIds)
            {
                if (!currentSupplierIds.Contains(supplierId))
                {
                    await _unitOfWork.SupplierProducts.AddAsync(new SupplierProduct
                    {
                        SupplierID = supplierId,
                        ProductID = existingProduct.ProductID
                    });
                }
            }

            await _unitOfWork.Save();
        }


        public async Task DeleteAsync(Guid id)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(e => e.ProductID == id);
            if (existingProduct == null)
            {
                throw new Exception("Product not found");
            }
            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}
