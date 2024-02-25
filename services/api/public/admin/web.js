function post(url, data) {
	return new Promise(function(res, rej) {
		$.ajax({
                    type: "POST",
                    url: "/admin-api/api/" + url,
                    data: data,
                    headers: {
                        'x-csrf-token': csrf,
                    },
                    complete: function(xhr, status) {
                        if (xhr.status == 200 ) {
                            return res();
                        }
                        let newCsrf = xhr.getResponseHeader("x-csrf-token");
                        if (newCsrf) {
                            csrf = newCsrf;
                            post(url, data).then(function(result) {
								res(result);
							}).catch(function(err) {
								rej(err);
							});
                        }else{
							$('#errorMessage').text(xhr.responseText);
							rej(xhr);
                        }
                    },
                    dataType: "json"
                });
	});
}