using AgriPata.Business.Interfaces;
using AgriPata.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriPata.Utility;
using AgriPata.Domain;
using AgriPata.Repository.Entities;
using System.Linq.Expressions;

namespace AgriPata.Business
{
    public class ProductBusiness : IProductBusiness
    {
        //declared the unitofwork as private member to access only in same region
        private readonly IUnitOfWork unitOfWork;
        // defined the class constructor, used to inject the unitofwork object.
        public ProductBusiness(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }
       // method used to return the single product fetched from db based on the id of product.
        public ResultModel<ProductModel> GetProduct(int productId)
        {
            // initialize the object from ResultModel which is a generic class to define the common return type.
            var methodResult = new ResultModel<ProductModel>();
            // calling function to get the product where id is matched and product is not deleted.
            var product = GetProduct(x => x.ProductId == productId && x.IsDeleted == false);
            //checking if product is not null
            if (product.IsNotNull())
            {
                //used automapper(Object to Object mapper) to copy data into Prodjct Model class object.
                var productModel = AutoMapper.Mapper.Map<ProductModel>(product);
                //used automapper(Object to Object mapper) to copy data into Property model object
                productModel.Tags = AutoMapper.Mapper.Map<List<ProductPropertyModel>>(product.ProductProperties);
                methodResult.Data = productModel;
            }
            return methodResult;
        }
        //method is used to return the list of all products stored in database.
        public ResultModel<List<ProductModel>> GetProducts()
        {
            // initialize the object from ResultModel which is a generic class to define the common return type.
            var methodResult = new ResultModel<List<ProductModel>>();
            //used to handle the exception
            try
            {
                //  getting all the products where product is not deleted.
                var products = unitOfWork.ProductRepository.GetAll(x => x.IsDeleted != true);
                methodResult.Data = new List<ProductModel>();
                //used automapper(Object to Object mapper) to copy data into Prodjct Model class object.
                AutoMapper.Mapper.Map(products, methodResult.Data);
            }
            catch (Exception ex)
            {
                methodResult.Error = true;
                methodResult.Message = ex.GetInnermostException();
            }
            return methodResult;
        }
        //method is used to return the list of products for specific user stored in database.
        public ResultModel<List<ProductModel>> GetProducts(int userId)
        {// initialize the object from ResultModel which is a generic class to define the common return type.
            var methodResult = new ResultModel<List<ProductModel>>();
            //used to handle the exception
            try
            {//  getting all the products where product is created by specific user and product is not deleted.
                var products = unitOfWork.ProductRepository.GetAll(x => x.IsDeleted != true && x.CreatedBy == userId);
                methodResult.Data = new List<ProductModel>();
                //used automapper(Object to Object mapper) to copy data into Prodjct Model class object.
                AutoMapper.Mapper.Map(products, methodResult.Data);
            }
            catch (Exception ex)
            {
                methodResult.Error = true;
                methodResult.Message = ex.GetInnermostException();
            }
            return methodResult;
        }
        //method is used to create and update the existing product.
        public ResultModel<ProductModel> SaveProduct(ProductModel productModel, bool isAdmin)
        {// initialize the object from ResultModel which is a generic class to define the common return type.
            var methodResult = new ResultModel<ProductModel>();
            //used to handle the exception
            try
            {
                //declare the product object
                Product product = null;
                //check if productModel is null or not
                if (productModel.IsNotNull())
                {
                    // check if record is existing product and need to be updated
                    if (productModel.ProductId > 0)
                    {
                        //updating the values of existing record
                        product = UpdateProduct(productModel, isAdmin);
                    }
                    else
                    {
                        //creating a new product
                        product = CreateProduct(productModel);
                    }
                    AutoMapper.Mapper.Map(product, methodResult.Data);
                }
            }
            catch (Exception ex)
            {
                methodResult.Error = true;
                methodResult.Message = ex.GetInnermostException();
            }
            return methodResult;
        }
        // method is used to create new product
        private Product CreateProduct(ProductModel productModel)
        {
            //intialize the product object
            var product = new Product();
            //copied data from productmodel to product object.
            AutoMapper.Mapper.Map(productModel, product);
            // copied product properties 
            product.ProductProperties = AutoMapper.Mapper.Map<List<ProductProperty>>(productModel.Tags);
            // copied product properties 
            product.ProductImages = AutoMapper.Mapper.Map<List<ProductImage>>(productModel.Images);
            //created a unique Add id based on the datetime
            product.AddId = Convert.ToInt64(DateTime.UtcNow.ToString("ddMMyyyyHHmmss"));            
            product.CreatedDate = DateTime.UtcNow;
            foreach (var prop in product.ProductProperties)
            {//setting a current date for each property
                prop.CreatedDate = DateTime.UtcNow;
            }
            // setting is deleted false as it is new record
            product.IsDeleted = false;
            //saving a record into database.
            unitOfWork.ProductRepository.Insert(product);

            return product;
        }
        //method is used to update the product, required two parameter productmodel and the type of user.
        private Product UpdateProduct(ProductModel productModel, bool isAdmin)
        {// getting the product by product id from db.
            var product = GetProduct(x => x.ProductId == productModel.ProductId);
            //checking if product exist
            if (product.IsNotNull())
            {
               // deleting all existing product properties
                unitOfWork.ProductPropertyRepository.DeleteAll(unitOfWork.ProductPropertyRepository.GetAll(x => x.ProductId == productModel.ProductId).ToList(), false);
                //deleting existing product images
                unitOfWork.ProductImageRepository.DeleteAll(unitOfWork.ProductImageRepository.GetAll(x => x.ProductId == productModel.ProductId).ToList(), false);
                // copied product properties 
                product.ProductProperties = AutoMapper.Mapper.Map<List<ProductProperty>>(productModel.Tags);
                foreach (var prop in product.ProductProperties)
                {
                    prop.CreatedDate = DateTime.UtcNow;
                }
                // copied product images 
                product.ProductImages = AutoMapper.Mapper.Map<List<ProductImage>>(productModel.Images);
                product.Name = productModel.Name;
                product.PriceNegotiable = productModel.PriceNegotiable;
                product.PricePerUnit = productModel.PricePerUnit;
                product.AdditionInformation = productModel.AdditionInformation;
                product.AmountForSale = productModel.AmountForSale;
                product.Country = productModel.Country;
                product.CurrencyId = productModel.CurrencyId;
                product.Description = productModel.Description;
                product.IsActive = productModel.IsActive;
                product.CategoryId = productModel.CategoryId;
                product.SubCategoryId = productModel.SubCategoryId;
                product.City = productModel.City;
                //checing if user is admin then make required changes 
                if (isAdmin)
                {
                    product.FeaturedProducts = productModel.FeaturedProducts;
                    product.BestSeller = productModel.BestSeller;
                    product.MostViewed = productModel.MostViewed;
                    product.OnSaleProducts = productModel.OnSaleProducts;
                    product.NewArriavals = productModel.NewArriavals;
                    product.DailyDeals = productModel.DailyDeals;
                   
                }
                // product is updated in db here
                unitOfWork.ProductRepository.Update(product);
            }
            return product;
        }
        //method to return the single product based on the filter
        private Product GetProduct(Expression<Func<Product, bool>> filter)
        {
            var Product = unitOfWork.ProductRepository.SingleOrDefault(filter);
            return Product;
        }
        //mothod to mark the product as deleted
        public ResultModel<bool> DeleteProduct(int productId)
        {
            var methodResult = new ResultModel<bool>();
            try
            {
                var Product = GetProduct(x => x.ProductId == productId);
                if (Product.IsNotNull())
                {
                    Product.IsDeleted = true;
                    unitOfWork.ProductRepository.Update(Product);

                    methodResult.Data = true;
                }

            }
            catch (Exception ex)
            {
                methodResult.Error = true;
                methodResult.Message = ex.GetInnermostException();
            }
            return methodResult;
        }
        //method to check if product name already exist.
        public ResultModel<ProductModel> CheckIfProdctNameExists(ProductModel productModel)
        {
            var methodResult = new ResultModel<ProductModel>();
          
              var  product = GetProduct(x => x.Name.ToLower() == productModel.Name.ToLower() && x.ProductId != productModel.ProductId && !x.IsDeleted);
                if (product.IsNotNull())
                {
                    methodResult.Error = true;
                    methodResult.Message = "Product Name already exist";
                }
            
            return methodResult;
        }
        //method to return all properties of product which are defined in the templates.
        public ResultModel<List<ProductPropertyModel>> GetProductProperties(int productId, int templateId)
        {
            var methodResult = new ResultModel<List<ProductPropertyModel>>();
            var properties = unitOfWork.TemplatePropertyRepository.GetAll(x => x.TemplateId == templateId && x.IsDeleted != true);
            var productProperties = unitOfWork.ProductPropertyRepository.GetAll(x => x.ProductId == productId);
            var propertiesModel = new List<ProductPropertyModel>();
            foreach (var prop in properties)
            {
                var productProp = productProperties.SingleOrDefault(x => x.TemplatePropertyId == prop.TemplatePropertyId);
                string propValue = string.Empty;
                if (productProp != null)
                {
                    propValue = productProp.PropertyValue;
                }
                var propModel = new ProductPropertyModel
                {
                    PropertyValue = propValue,
                    TemplatePropertyId = prop.TemplatePropertyId,
                    PropertyName = prop.PropertyName,
                    DataType = prop.DataType,
                    ProductId = productId,
                    IsRequired = prop.IsRequired

                };
                propertiesModel.Add(propModel);
            }
            methodResult.Data = propertiesModel;
            return methodResult;
        }
        //method is used to return all the images of product.
        public ResultModel<List<ProductImageModel>> GetProductImages(int productId)
        {
            var methodResult = new ResultModel<List<ProductImageModel>>();
            var productImages = unitOfWork.ProductImageRepository.GetAll(x => x.ProductId == productId);
            methodResult.Data = new List<ProductImageModel>();
            AutoMapper.Mapper.Map(productImages, methodResult.Data);
            return methodResult;
        }
        //method is used to get the list of special products which are active and not deleted.
        public ResultModel<List<PublicProductModel>> GetOfferedProducts()
        {
            var methodResult = new ResultModel<List<PublicProductModel>>();
            try
            {
                var products = unitOfWork.ProductRepository.GetAll(x => x.IsDeleted != true && x.IsActive == true && (x.FeaturedProducts || x.BestSeller || x.MostViewed || x.OnSaleProducts || x.NewArriavals)).ToList();
                methodResult.Data = new List<PublicProductModel>();
                AutoMapper.Mapper.Map(products, methodResult.Data);
            }
            catch (Exception ex)
            {
                methodResult.Error = true;
                methodResult.Message = ex.GetInnermostException();
            }
            return methodResult;

        }
        //get products by subcategory
        public ResultModel<List<PublicProductModel>> GetOfferedProductsBySubCategory(int subCategoryId)
        {
            var methodResult = new ResultModel<List<PublicProductModel>>();
            try
            {
                var products = unitOfWork.ProductRepository.GetAll(x => x.IsDeleted != true && x.IsActive == true && x.SubCategoryId==subCategoryId).ToList();
                methodResult.Data = new List<PublicProductModel>();
                AutoMapper.Mapper.Map(products, methodResult.Data);
            }
            catch (Exception ex)
            {
                methodResult.Error = true;
                methodResult.Message = ex.GetInnermostException();
            }
            return methodResult;

        }
        //get the detail of offered product by id. It should return a public model with specific properties
        public ResultModel<PublicProductModel> GetOfferedProduct(int productId)
        {

            var methodResult = new ResultModel<PublicProductModel>();
            try
            {
                var products = unitOfWork.ProductRepository.SingleOrDefault(x => x.IsDeleted != true && x.IsActive == true && x.ProductId == productId);
                methodResult.Data = new PublicProductModel();
                AutoMapper.Mapper.Map(products, methodResult.Data);
            }
            catch (Exception ex)
            {
                methodResult.Error = true;
                methodResult.Message = ex.GetInnermostException();
            }
            return methodResult;

        }
    }
}
