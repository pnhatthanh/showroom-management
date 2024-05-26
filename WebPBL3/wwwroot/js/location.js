$(document).ready(function () {
    GetProvince();
});
function GetProvince() {
    $.ajax({
        url: '/Staff/GetProvince',
        success: function (result) {
            $.each(result, function (i,data) {
                $('#ProvinceName').append('<option value =' + data.id + '>' + data.name + '</option>');
            });
        }
    });
}