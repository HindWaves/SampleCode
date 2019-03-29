using AgriPata.API.ViewModels;
using AgriPata.Business.Interfaces;
using AgriPata.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AgriPata.API.utility;
using AgriPata.API.ViewModels.ResponseModels;
using AgriPata.Utility;

namespace AgriPata.API.Controllers
{// Authorize attribute is used to prevent the anonymous user access 
    [Authorize]
    //defined prefix route for Api
    [RoutePrefix("api/product")]
    public class ProductController : BaseApiController
    {
        //declared product business object as private
        private IProductBusiness ProductBusiness;
        //inject the product business interface to access the methods of it.
        public ProductController(IProductBusiness _ProductBusiness)
        {
            //initialize the product business object
            ProductBusiness = _ProductBusiness;
        }
        //method to get the product by id
        [HttpGet, Route("GetProduct/{productId}")]
        public HttpResponseMessage GetProduct(int productId)
        {
            var result = ProductBusiness.GetProduct(productId);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        //method to get the list of products 
        [HttpGet, Route("GetProducts")]
        public HttpResponseMessage GetProducts()
        {
            var result = new ResultModel<List<ProductModel>>();
            //checking if user type is Admin to display all the records
            if (User.Identity.IsAdminUser())
            {
                result = ProductBusiness.GetProducts();
            }
            else
            {
                result = ProductBusiness.GetProducts(CurrentContext.CurrentUserId);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        //method is used to create or update the product.
        [HttpPost]
        [Route("SaveProduct")]
        public HttpResponseMessage SaveProduct(ProductModel productModel)
        {
            var isAdminUser = User.Identity.IsAdminUser();
            productModel.CreatedBy = CurrentContext.CurrentUserId;
            var result = ProductBusiness.CheckIfProdctNameExists(productModel);
            if (!result.Error)
                result = ProductBusiness.SaveProduct(productModel, isAdminUser);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        
        //method is used to get the product properties
        [HttpGet]
        [Route("GetProperties/{productId}/{templateId}")]
        public HttpResponseMessage GetProperties(int productId, int templateId)
        {
            var result = ProductBusiness.GetProductProperties(productId, templateId);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        //method is used to get the product images
        [HttpGet]
        [Route("GetImages/{productId}")]
        public HttpResponseMessage GetImages(int productId)
        {
            var methodResult = ProductBusiness.GetProductImages(productId);
            var result = new ResultViewModel<List<ImageViewModel>>();
            AutoMapper.Mapper.Map(methodResult, result);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        // method is used to get the special products for Anonymous user.
        [HttpGet, AllowAnonymous, Route("GetOfferedProducts")]
        public HttpResponseMessage GetOfferedProducts()
        {
            var methodResult = new ResultModel<List<PublicProductModel>>();
            methodResult = ProductBusiness.GetOfferedProducts();
            var result = new ResultViewModel<List<PublicProductViewModel>>();
            AutoMapper.Mapper.Map(methodResult, result);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
     // method is used to get the special product by id for Anonymous user.
        [HttpGet, AllowAnonymous, Route("GetOfferedProduct/{productId}")]
        public HttpResponseMessage GetOfferedProduct(int productId)
        {
            var methodResult = new ResultModel<PublicProductModel>();
            methodResult = ProductBusiness.GetOfferedProduct(productId);
            var result = new ResultViewModel<PublicProductViewModel>();
            AutoMapper.Mapper.Map(methodResult, result);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        // method is used to get the products by subcategory for Anonymous user.
        [HttpGet, AllowAnonymous, Route("GetOfferedProductsBySubCategory/{subCategoryId}")]
        public HttpResponseMessage GetOfferedProductsBySubCategory(int subCategoryId)
        {
            var methodResult = new ResultModel<List<PublicProductModel>>();
            methodResult = ProductBusiness.GetOfferedProductsBySubCategory(subCategoryId);
            var result = new ResultViewModel<List<PublicProductViewModel>>();
            AutoMapper.Mapper.Map(methodResult, result);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        // method is used to send the abuse report to admin by mail.
        [HttpPost, AllowAnonymous, Route("SendReport")]
        public HttpResponseMessage SendReport(PublicProductModel productModel)
        {           
            var result = new ResultViewModel<bool>();
            
            var file = HtmlTemplatePath  + "ProductAbuse.txt";
            var mailTemplate = Common.ReadTextFile(file);
            var product = ProductBusiness.GetProduct(productModel.ProductId);
            if (product.Data != null)
            {
                var template = Common.Render(product.Data, mailTemplate);
              
                result.Data = new Mailer()
                                       .To(ConfigMgr.AdminEmailAddress)
                                       .Subject("Product Abused by User")
                                       .Body(template)
                                       .Send();
              
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

    }
}
