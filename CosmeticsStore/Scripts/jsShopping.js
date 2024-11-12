$(document).ready(function () {
    ShowCount();
    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quatity = 1;
        var tQuantity = $('#quantity_value').text();
        if (tQuantity != '') {
            quatity = parseInt(tQuantity);
        }

        //alert(id + " " + quatity);
        $.ajax({
            url: '/shoppingcart/addtocart',
            type: 'POST',
            data: { id: id, quantity: quatity },
            success: function (rs) {
                if (rs.Success) {
                    $('#checkout_items').html(rs.Count);
                    swal({
                        title: 'Thông báo',
                        text: rs.msg,
                        icon: 'success',
                        button: 'Đóng'
                    });
                }
                else {
                    swal({
                        title: 'Thông báo',
                        text: 'Sản phẩm trong kho không đủ!',
                        icon: 'warning',
                        button: 'Đóng'
                    });
                }
            }
        });
    });

    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        swal({
            title: 'Thông báo',
            text: "Bạn có chắc muốn xóa sản phẩm khỏi giỏ hàng?",
            icon: 'warning',
            buttons: true,
            dangerMode: true
        }).then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    url: '/shoppingcart/Delete',
                    type: 'POST',
                    data: { id: id },
                    success: function (rs) {
                        if (rs.Success) {
                            $('#checkout_items').html(rs.Count);
                            $('#trow_' + id).remove();
                            LoadCart();
                        }
                    }
                });
            }
        })

    });

    $('body').on('click', '.btnDeleteAll', function (e) {
        e.preventDefault();
        swal({
            title: 'Thông báo',
            text: 'Bạn có chắc muốn xóa tất cả sản phẩm khỏi giỏ hàng?',
            icon: 'warning',
            buttons: true,
            dangerMode: true
        }).then((willDelete) => {
            if (willDelete) {
                DeleteAll();
            }
        })
    });

    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var id = $(this).data("id")
        var quantity = $('#Quantity_' + id).val();
        CheckQuantity(id, quantity)
        
    })

});

function ShowCount() {
    $.ajax({
        url: '/shoppingcart/ShowCount',
        type: 'GET',
        success: function (rs) {
            $('#checkout_items').html(rs.Count);
        }
    });
}

function DeleteAll() {
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}

function Update(id, quantity) {
    $.ajax({
        url: '/shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}
function CheckQuantity(id, quantity) {
    $.ajax({
        url: '/shoppingcart/CheckQuantity',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                Update(id, quantity);
            }
            else {
                Update(id, (quantity - 1));
                swal({
                    title: 'Thông báo',
                    text: 'Sản phẩm trong kho chỉ còn lại: ' + (quantity - 1) + ' sản phẩm!',
                    icon: 'error',
                    button: 'Đóng'
                });
            }
        }
    });
}
function LoadCart() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart',
        type: 'GET',
        success: function (rs) {
            $('#load_data').html(rs);
        }
    });
}