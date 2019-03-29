using AgriPata.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriPata.Business.Interfaces
{
    public interface IProductBusiness
    {
        ResultModel<ProductModel> GetProduct(int productId);
        ResultModel<List<ProductModel>> GetProducts();
        ResultModel<List<ProductModel>> GetProducts(int userId);
        ResultModel<ProductModel> SaveProduct(ProductModel productModel,bool isAdmin);
        ResultModel<bool> DeleteProduct(int productId);
        ResultModel<ProductModel> CheckIfProdctNameExists(ProductModel productModel);
        ResultModel<List<ProductPropertyModel>> GetProductProperties(int productId, int templateId);
        ResultModel<List<ProductImageModel>> GetProductImages(int productId);
        ResultModel<List<PublicProductModel>> GetOfferedProducts();
        ResultModel<PublicProductModel> GetOfferedProduct(int productId);
        ResultModel<List<PublicProductModel>> GetOfferedProductsBySubCategory(int subCategoryId);
       // ResultModel<List<MasterCategoryModel>> GetMasterCategories(int productId);
    }
}
