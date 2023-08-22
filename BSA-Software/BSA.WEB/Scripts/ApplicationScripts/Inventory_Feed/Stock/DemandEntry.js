var Categories = [];
var sum = 0;


$(document).ready(function () {
    $('input[type=datetime]').datepicker({
        dateFormat: "dd-M-yy",
        changeMonth: true,
        changeYear: true,
        yearRange: "2001:+50"
    });
    buttonVisibility();

});


//fetch categories from database
function LoadCategory(element) {
    if (Categories.length === 0) {
        //ajax function for fetch data
        $.ajax({
            type: "GET",
            url: '/OrderMaster/GetProductCategories',
            success: function (data) {
                Categories = $.parseJSON(data);
                //render catagory
                renderCategory(element);
            }
        });
    }
    else {
        //render catagory to the element
        renderCategory(element);
    }
}
function renderCategory(element) {
    var $ele = $(element);
    $ele.empty();
    $ele.append($('<option/>').val('0').text('Select'));
    $.each(Categories, function (i, val) {
        $ele.append($('<option/>').val(val.Value).text(val.Text));
    });
}

//fetch product sub categories
function LoadProductSubCategory(elementProductCategory) {
    $.ajax({
        type: "GET",
        url: "/OrderMaster/GetProductSubCategories",
        data: { 'productCategoryId': $(elementProductCategory).val() },
        success: function (data) {
            var dataa = $.parseJSON(data);
            //render products to appropriate dropdown
            renderProductSubCategory($(elementProductCategory).parents('.mycontainer').find('select.psc'), dataa);
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function renderProductSubCategory(element, data) {
    //render product
    var $ele = $(element);
    $ele.empty();
    $ele.append($('<option/>').val('0').text('Select'));
    $.each(data, function (i, val) {
        $ele.append($('<option/>').val(val.Value).text(val.Text));
    });
}


//fetch products
function LoadProduct(elementProductSubCategory) {
    $.ajax({
        type: "GET",
        url: "/OrderMaster/GetProducts",
        data: { 'productSubCategoryId': $(elementProductSubCategory).val() },
        success: function (data) {
            var dataa = $.parseJSON(data);
            //render products to appropriate dropdown
            renderProduct($(elementProductSubCategory).parents('.mycontainer').find('select.product'), dataa);
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function renderProduct(element, data) {
    //render product
    var $ele = $(element);
    $ele.empty();
    $ele.append($('<option/>').val('0').text('Select'));
    $.each(data, function (i, val) {
        $ele.append($('<option/>').val(val.Value).text(val.Text));
    });
}
LoadCategory($('#productCategory'));



function addItemToGrid() {
    createRowForStock();
    buttonVisibility();
}

$(document).keypress(function (e) {
    if (e.which === 13) {
        addItemToGrid();
    }
});



function buttonVisibility() {
    var index = $('#itemDetails').children("tr").length;
    if (index === 0) {
        $("#submit").attr("disabled", true);
    }
    else {
        $("#submit").attr("disabled", false);
    }
}
function createRowForStock() {

    var isAllValid = true;


    if ($('#product').val() === "0" || $('#product').val() === "Select") {
        isAllValid = false;
        $('#product').siblings('span.error').css('visibility', 'visible');
    }
    else {
        $('#product').siblings('span.error').css('visibility', 'hidden');
    }

    if (!($('#qty').val().trim() !== '')) {
        isAllValid = false;
        $('#qty').siblings('span.error').css('visibility', 'visible');
    }
    else {
        $('#qty').siblings('span.error').css('visibility', 'hidden');
    }

    if (!($('#rate').val().trim() !== '')) {
        isAllValid = false;
        $('#rate').siblings('span.error').css('visibility', 'visible');
    }
    else {
        $('#rate').siblings('span.error').css('visibility', 'hidden');
    }

    if (isAllValid) {

        var selectedItems = getSelectedItems();
        console.log(selectedItems);
        var index = $('#itemDetails').children("tr").length;
        var sl = index;

        var indexCell = "<td style='display:none'> <input type='hidden' id='Index " + index + "' name='DemandDetails.Index' value='" + index + "' /> </td>";
        var serialCell = "<td>" + (++sl) + "</td>";
        var removeCell = "<td><input type='button' id='removeItem' class='remove' value='x'/> </td>";
        var prodectNameCell = "<td><input type='hidden' id='ProductId" + index + "' name='DemandDetails[" + index + "].ProductId' value='" + selectedItems.ProductId + "' />" + selectedItems.ProductName + " </td>";
        var unitCell = "<td><input type='hidden' id='unitId" + index + "' name='DemandDetails[" + index + "].UnitId' value='" + selectedItems.UnitId + "' />" + selectedItems.Unit + " </td>";
        var qtyCell = "<td><input type='hidden' id='Qty" + index + "' name='DemandDetails[" + index + "].Qty' value='" + selectedItems.Qty + "' />" + selectedItems.Qty + " </td>";
        var rateCell = "<td><input type='hidden' id='rate" + index + "' name='DemandDetails[" + index + "].Rate' value='" + selectedItems.Rate + "' />" + selectedItems.Rate + " </td>";
        //var categoryCell = "<td><input type='hidden' id='CategoryId" + index + "' name='StoreDetails[" + index + "].ProductCategoryId' value='" + selectedItems.CategoryId + "' />" + selectedItems.CategoryName + " </td>";
        //var subCategoryCell = "<td><input type='hidden' id='SubCategoryId" + index + "' name='StoreDetails[" + index + "].ProductSubCategoryId' value='" + selectedItems.SubCategoryId + "' />" + selectedItems.SubCategoryName + " </td>";
        var amountCell = "<td><input type='hidden' id='Amount" + index + "' name='DemandDetails[" + index + "].Amount' value='" + selectedItems.Amount + "' />" + selectedItems.Amount + " </td>";
        var createNewRow = "<tr id='" + (++sl) + "'>" + indexCell + serialCell + prodectNameCell + unitCell + qtyCell + rateCell + amountCell + removeCell + " </tr>";
        $('#itemDetails').append(createNewRow);

        //sum += selectedItems.Qty * selectedItems.UnitPrice;
        //$('#tamount').val(sum);

        $('#product').val('');
        $('#unit').val('');
        $('#qty').val('');
        $('#rate').val('');
        $('#amount').val('');
        $('#orderItemError').empty();
    }
}

$("body").on('click', '.remove', function () {

    var rid = $(this).closest('tr').attr('id');
    if (confirm("Are you sure to remove this ?")) {
        $("#" + rid).remove();
    }
    buttonVisibility();
});

function getSelectedItems() {
    var productId = $('#hfProductId').val();
    var productName = $('#product').val();
    var unit = $('#unit').val();
    var unitId = $('#hfUnitId').val();
    var qty = $('#qty').val();
    var rate = $('#rate').val();

    var amount = $('#amount').val();

    var item = {
        "ProductId": productId,
        "ProductName": productName,
        "Unit": unit,
        "UnitId": unitId,
        "Qty": qty,
        "Rate": rate,
        "Amount": amount
    };
    return item;
}

var disamount = 0;
$("#disrate").keyup(function () {

    var disrate = ($(this).val());
    var totalamount = $("#tamount").val();
    disamount = (totalamount * disrate) / 100
    $("#disamount").val(disamount);

    grandTotal();
});

$("#disamount").keyup(function () {

    grandTotal();
});


function grandTotal() {
    var grandTotal = 0;
    if ($("#disamount").val() >= 0) {
        var total = $("#tamount").val();
        var disAmount = $("#disamount").val();
        grandTotal = total - disAmount;
    }

    $("#grandtotal").val(grandTotal);
}

$('#ddlProductCategory').change(function () {
    $.ajax({
        type: "post",
        url: "/ProductSubCategories/GetProductSubCategorySelectModelsByProductCategory",
        data: { productCategoryId: $('#ddlProductCategory').val() },
        datatype: "json",
        traditional: true,
        success: function (data) {
            var ProductSubCategory = "<select>";
            ProductSubCategory = ProductSubCategory + '<option value="">--Select--</option>';
            for (var i = 0; i < data.length; i++) {
                ProductSubCategory = ProductSubCategory + '<option value=' + data[i].Value + '>' + data[i].Text + '</option>';
            }
            ProductSubCategory = ProductSubCategory + '</select>';
            $('#ddlProductSubCategory').html(ProductSubCategory);
        }
    });
});
$('#ddlProductSubCategory').change(function () {
    $.ajax({
        type: "post",
        url: "/Products/GetProductSelectModelsByProductSubCategory",
        data: { productSubCategoryId: $('#ddlProductSubCategory').val() },
        datatype: "json",
        traditional: true,
        success: function (data) {
            var Product = "<select>";
            Product = Product + '<option value="">--Select--</option>';
            for (var i = 0; i < data.length; i++) {
                Product = Product + '<option value=' + data[i].Value + '>' + data[i].Text + '</option>';
            }
            Product = Product + '</select>';
            $('#ddlProduct').html(Product);
        }
    });
});

var amount = 0;
$("#unitPrice").keyup(function () {


    var unitPrice = ($(this).val());
    var qty = $('#qty').val();
    amount = unitPrice * qty;
    $('#amount').val(amount);
});

$("#qty").keyup(function () {
    var qty = $(this).val();
    if (isNaN(qty)) {
        alert("Plese select valid number");
        $('#qty').val('');
    }

    //var unitPrice = $('#unitPrice').val();

    //$('#amount').val(unitPrice * qty);
});

$("#rate").keyup(function () {
    var qty = $(this).val();
    if (isNaN(qty)) {
        alert("Plese select valid number");
        $('#rate').val('');
    }
});

$('#customerId').change(function () {

    var cusId = $(this).val();

    $.ajax({
        type: "GET",
        url: "/OrderMaster/GetCustomerInfo",
        data: { 'id': cusId },
        success: function (data) {
            console.log(data);
            $('#CompanyName').val(data.Name);
            $('#Address').val(data.Address);


        },
        error: function (error) {
            console.log(error);
        }
    });

});