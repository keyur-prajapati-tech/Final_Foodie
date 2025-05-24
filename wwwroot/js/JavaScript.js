<script>
    let selectedAddressId = 0;

    function updateSelectedAddress(addressId) {
        selectedAddressId = addressId;
        console.log("Selected Address ID:", selectedAddressId);
    }


    function makePayment(addressId) {
        if(!selectedAddressId) {
            alert("Please select a delivery address.");
            return;
        }else{
            console.log("Proceeding with Address ID:", addressId);

            var orderItems = [];
            var totalAmount = 0;
            var isFromCart = false;

            if ($(".cart-item").length > 0) {
                isFromCart = true;

                $(".cart-item").each(function () {
                    const productId = $(this).data('order_items_id');
                    const quantity = parseInt($(this).find('.quantity').text());
                    const price = parseFloat($(this).data('amount'));

                    totalAmount += amount * quantity;

                    orderItems.push({
                        ProductId: orderItems,
                    Quantity: quantity,
                    Price: amount
                 });
            });
        }

        fetch('/displayCart/InitiateOrder', {
            method: 'POST',
        headers: {'Content-Type': 'application/json' },
        body: JSON.stringify({
            Amount: grandTotal,
        orderItems: orderItems,
        AddressId: addressId
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.orderId) {
                var options = {
            "key": "rzp_test_uIxZbgotqYKcAK",
        "amount": grandTotal * 100,
        "currency": "INR",
        "name": "Foodie Order",
        "description": "Tasty Food Order Payment",
        "order_id": data.orderId,
        "handler": function (response) {
                        var paymentData = {
            razorpay_payment_id: response.razorpay_payment_id,
        razorpay_order_id: response.razorpay_order_id,
        razorpay_signature: response.razorpay_signature,
        amount: grandTotal,
        addressId: addressId,
        isFromCart: isFromCart
     };

    fetch('/displayCart/Success', {
        method: 'POST',
        headers: {'Content-Type': 'application/json' },
        body: JSON.stringify(paymentData)
                        })
                        .then(res => res.json())
                        .then(response => {
                            if (response.success) {
            alert('Payment successful!');
        window.location.href = '/displayCart/ThankYou';
                            } else {
            alert('Payment verification failed');
                            }
                        });
                    },
        "prefill": {
            "name": "Test User",
            "email": "test@example.com",
            "contact": "9909817574"
         }
      };

    var rzp1 = new Razorpay(options);
    rzp1.open();
        } else {
        alert('Error creating Razorpay order: ' + data.error);
        }
    });
}
</script>