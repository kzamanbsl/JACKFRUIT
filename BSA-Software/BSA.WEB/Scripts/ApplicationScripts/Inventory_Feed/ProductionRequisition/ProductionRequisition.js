$(document).ready(function () {
    $('input[type=datetime]').datepicker({
        dateFormat: "dd/M/yy",
        changeMonth: true,
        changeYear: true,
        yearRange: "2001:+50"
    });
    EnterToTab();
    buttonVisibility();
    //orderNo();

});


function EnterToTab() {
    var inputs = $(':input').keypress(function (e) {
        if (e.which === 13) {
            e.preventDefault();
            var nextInput = inputs.get(inputs.index(this) + 1);
            if (nextInput) {
                nextInput.focus();
            }
        }
    });
}


$('#add').click(function () {

    createRowForStock();
    buttonVisibility();
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


    if ($('#product').val() === "0" || ($('#product').val() === "Select")) {
        isAllValid = false;
        $('#product').siblings('span.error').css('visibility', 'visible');
    }
    else {
        $('#product').siblings('span.error').css('visibility', 'hidden');
    }

    if (!($('#qty').val().trim() !== '' && (parseInt($('#qty').val()) || 0))) {
        isAllValid = false;
        $('#qty').siblings('span.error').css('visibility', 'visible');
    }
    else {
        $('#qty').siblings('span.error').css('visibility', 'hidden');
    }

    if (isAllValid) {

        var selectedItems = getSelectedItems();
        var index = $('#itemDetails').children("tr").length;
        var sl = index;

        var indexCell = "<td style='display:none'> <input type='hidden' id='Index " + index + "' name='RequisitionItems.Index' value='" + index + "' /> </td>";
        var serialCell = "<td>" + (++sl) + "</td>";
        var removeCell = "<td><input type='button' id='removeItem' class='remove' value='x'/> </td>";
        var prodectNameCell = "<td><input type='hidden' id='ProductId" + index + "' name='RequisitionItems[" + index + "].ProductId' value='" + selectedItems.ProductId + "' />" + selectedItems.ProductName + " </td>";
        var qtyCell = "<td><input type='hidden' id='Qty" + index + "' name='RequisitionItems[" + index + "].Qty' value='" + selectedItems.Qty + "' />" + selectedItems.Qty + " </td>";
       
       
        var createNewRow = "<tr id='" + (++sl) + "'>" + indexCell + serialCell+prodectNameCell + qtyCell + removeCell + " </tr>";
        $('#itemDetails').append(createNewRow);

        $('#product').val('');
        $('#qty').val('');
        $('#orderItemError').empty();
    }
}


$("body").on('click', '.remove', function () {

    var rid = $(this).closest('tr').attr('id');
    var rtam = $('#Amount' + (rid - 2)).val();
    var tam = $('#tamount').val();
    var ntam = tam - rtam;

    if (confirm("Are you sure to remove this ?")) {
        $("#" + rid).remove();

    }
    buttonVisibility();
});

function getSelectedItems() {

    //var catId = $('#productCategory option:selected').val();
    //var catName = $('#productCategory option:selected').text();
    //var subCatId = $('#productSubCategory option:selected').val();
    //var subCatName = $('#productSubCategory option:selected').text();
    var productId = $('#hfProductId').val();
    var productName = $('#product').val();
    var qty = $('#qty').val();
  
    var item = {
        
        "ProductId": productId,
        "ProductName": productName,
        "Qty": qty
       
    };
    return item;
}















