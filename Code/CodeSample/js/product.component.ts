import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonService } from 'src/services/common.service';
import { ProductService } from 'src/services/product.service';
import { ProductModel } from 'src/models/product';
import { first } from 'rxjs/operators';
import { ToastrService } from 'src/services/toastr.service';
import { MatDialog, MatDialogConfig } from "@angular/material";
import { FlxUiDataTable } from 'flx-ui-datatable';

@Component({
    selector: 'app-product',
    templateUrl: './product.component.html',
    styleUrls: ['./product.component.css']
})
export class ProductComponent implements OnInit {
    categoryList: [];
    dtOptions: DataTables.Settings = {};
    productModel: ProductModel;
    listUrl: any;
    constructor(private _productService: ProductService,
        private router: Router,
        private toastr: ToastrService,
        private dialog: MatDialog,
        private dataTableService: FlxUiDataTable,
        private _commonService: CommonService) {
        this.listUrl = this._commonService.getBaseUrl() + 'product/GetProducts?token='+localStorage.getItem("token");
    }

    ngOnInit(): void {
        this.dtOptions = {
            pagingType: 'full_numbers',
            paging: true,
            searching: true,
            pageLength: 10,
            displayStart: 1,
            lengthMenu: [5, 10, 25, 50, 100],
        };
        this.getProduct();
    }

    getProduct() {
        this._productService.getProduct()
            .pipe(first())
            .subscribe(
                data => {
                    if (data.Data.length) {
                        this.dtOptions.data = data.Data;
                    }
                },
                error => { });
    };

    EditProduct(event) {
        this.productModel = event.data
        if (this.productModel != null) {
            this.router.navigate(['Admin/Product', this.productModel.ProductId]);
        }
    }

    DeletePopUp(event) {
        this.productModel = event.data
        if (this.productModel != null) {
            this.DeleteProduct()
        }
    }

    DeleteProduct() {
        this._productService.deleteProduct(this.productModel.ProductId)
            .pipe(first())
            .subscribe(
                result => {
                    if (result.Data) {
                        this.dataTableService.reloadData()
                        this.toastr.showSuccess("Product Deleted Successfully")
                    } else
                        this.toastr.showError("Error deleting Product")
                },
                error => {
                    this.toastr.showError("Something went wrong while deleting Product")
                });
    };

}
