using UmkmCRUD.Services.Interfaces;
using UmkmCRUD.Repository.Interfaces;
using UmkmCRUD.Common;
using AutoMapper;

namespace UmkmCRUD.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<ProductResponse>>> GetProducts()
        {
            var products = await _productRepository.GetAllAsync();

            var response = _mapper.Map<IEnumerable<ProductResponse>>(products);

            return ServiceResult<IEnumerable<ProductResponse>>.Success(response);
        }

        public async Task<ServiceResult<ProductResponse>> CreateProduct(ProductRequest request)
        {
            if (!request.CategoryId.HasValue || request.CategoryId.Value == Guid.Empty)
            {
                return ServiceResult<ProductResponse>.Fail(
                    new ServiceError(ErrorType.Validation, "CategoryId is required"));
            }

            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId.Value);
            if (!categoryExists)
            {
                return ServiceResult<ProductResponse>.Fail(
                    new ServiceError(ErrorType.NotFound, "Category not found"));
            }

            var product = _mapper.Map<Product>(request);

            if (request.Stock.HasValue)
                product.Stock = request.Stock.Value;

            await _productRepository.AddAsync(product);

            await _productRepository.LoadCategoryAsync(product);

            var response = _mapper.Map<ProductResponse>(product);

            return ServiceResult<ProductResponse>.Success(response);
        }


        public async Task<ServiceResult<ProductResponse>> GetProductById(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return ServiceResult<ProductResponse>.Fail(new ServiceError(ErrorType.NotFound, "Product not found"));
            }

            var response = _mapper.Map<ProductResponse>(product);

            return ServiceResult<ProductResponse>.Success(response);
        }

        public async Task<ServiceResult<bool>> DeleteProduct(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<bool>.Fail(new ServiceError(ErrorType.NotFound, "Product not found"));
            }

            await _productRepository.DeleteAsync(product);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<ProductResponse>> UpdateProduct(Guid id, ProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return ServiceResult<ProductResponse>.Fail(new ServiceError(ErrorType.NotFound, "Product not found"));
            }

            if (!string.IsNullOrWhiteSpace(request.ProductName))
            {
                product.ProductName = request.ProductName;
            }

            if (request.Stock.HasValue)
            {
                product.Stock = request.Stock.Value;
            }

            if (request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId)
            {
                var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId.Value);
                if (!categoryExists)
                {
                    return ServiceResult<ProductResponse>.Fail(new ServiceError(ErrorType.NotFound, "Category not found"));
                }
                product.CategoryId = request.CategoryId.Value;
            }

            await _productRepository.UpdateAsync(product);
            await _productRepository.LoadCategoryAsync(product);

            var response = _mapper.Map<ProductResponse>(product);
            return ServiceResult<ProductResponse>.Success(response);
        }
    }
}
