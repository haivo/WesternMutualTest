import { expect, test } from '@playwright/test';
let apiContext;
test.beforeAll(async ({ playwright }) => {
  apiContext = await playwright.request.newContext({
    baseURL: "http://localhost:5116",
    ignoreHTTPSErrors: true
  });
})
test.afterAll(async ({ }) => {
  await apiContext.dispose();
})

test('add guid and expire success', async ({ page }) => {
  const httpResult = await apiContext.post("/guid", {
    data: {
      "user": "Joe, Smith."
    }
  })
  const apiResult = await httpResult.json();
  expect(httpResult.status()).toBe(200);

  const httpDeleteResult = await apiContext.delete("/guid/" + apiResult.guid)
  expect(httpDeleteResult.status()).toBe(200);
});
test('add guid expired failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid", {
    data: {
      "expire": "1427736345",
      "user": "Joe, Smith."
    }
  })
  const apiResult = await httpResult.json();
  expect(httpResult.status()).toBe(400);
  expect(apiResult.message).toEqual("Field 'expire' was expired.")
});

test('add guid with 35 chars failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69AAAAAA", {
    data: {
      "expire": "1427736345",
      "user": "Joe, Smith."
    }
  })
  const apiResult = await httpResult.json();
  expect(httpResult.status()).toBe(400);
  expect(apiResult.message).toEqual("GUID is invalid format.")
});

test('add valid guid valid expire success', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69DDF", {
    data: {
      "expire": "1685358249",
      "user": "Joe, Smith."
    }
  })

  const apiResult = await httpResult.json();
  expect(httpResult.status()).toBe(200);
  const httpDeleteResult = await apiContext.delete("/guid/9094E4C980C74043A4B586B420E69DDF")
  expect(httpDeleteResult.status()).toBe(200);
});


test('add guid without expire delete success', async ({ page }) => {
  const httpAddResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69DD7", {
    data: {
      "user": "Joe, Smith."
    }
  })
  expect(httpAddResult.status()).toBe(200);
  const httpDeleteResult = await apiContext.delete("/guid/9094E4C980C74043A4B586B420E69DD7")
  expect(httpDeleteResult.status()).toBe(200);
});

test('add without guid and expire delete success', async ({ page }) => {
  const httpAddResult = await apiContext.post("/guid/", {
    data: {
      "user": "Joe, Smith."
    }
  })
  expect(httpAddResult.status()).toBe(200);
});

test('add guid with null expire failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/", {
    data: {
      "expire": null,
      "user": "Joe, Smith."
    }
  })
  const apiResult = await httpResult.json();
  expect(httpResult.status()).toBe(400);
  expect(apiResult.message).toEqual("Field 'expire' is not a valid date.")
});

test('add guid with empty body failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69DD1")
  expect(httpResult.status()).toBe(400);
  const apiResult = await httpResult.json();
  expect(apiResult.message).toEqual("Metadata is required.")
});

test('add guid with wrong characters failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69XXX")
  expect(httpResult.status()).toBe(400);
  const apiResult = await httpResult.json();
  expect(apiResult.message).toEqual("GUID is invalid format.")
});

test('add guid with length less than 32 failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69DD")
  expect(httpResult.status()).toBe(400);
  const apiResult = await httpResult.json();
  expect(apiResult.message).toEqual("GUID is invalid format.")
});

test('add guid with valid expire without metadata failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69DDF",
    {
      data: {
        "expire": "1685390665"
      }
    })
  expect(httpResult.status()).toBe(400);
  const apiResult = await httpResult.json();
  expect(apiResult.message).toEqual("Metadata is required.")
});

test('add guid without guid and metadata failed', async ({ page }) => {
  const httpResult = await apiContext.post("/guid/")
  expect(httpResult.status()).toBe(400);
  const apiResult = await httpResult.json();
  expect(apiResult.message).toEqual("Metadata is required.")
});

test('add get delete success', async ({ page }) => {
  const httpAddResult = await apiContext.post("/guid/9094E4C980C74043A4B586B420E69DD8", {
    data: {
      "expire": "1685388429",
      "user": "Joe, Smith."
  }
  })
  expect(httpAddResult.status()).toBe(200);

  const httpGetResult = await apiContext.get("/guid/9094E4C980C74043A4B586B420E69DD8")
  expect(httpGetResult.status()).toBe(200);
  const apiResult = await httpGetResult.json();
  expect(apiResult.user).toEqual("Joe, Smith.")
  expect(apiResult.expire).toEqual("1685388429")
  expect(apiResult.guid).toEqual("9094E4C980C74043A4B586B420E69DD8")

  const httpDeleteResult = await apiContext.delete("/guid/9094E4C980C74043A4B586B420E69DD8")
  expect(httpDeleteResult.status()).toBe(200);

});

test('get guid not found failed', async ({ page }) => {
  const httpResult = await apiContext.get("/guid/9094E4C980C74043A4B586B420E69AAA")
  expect(httpResult.status()).toBe(404);
  const apiResult = await httpResult.json();
  expect(apiResult.message).toEqual("This GUID cannot be found.")
});

test('delete invalid guid failed', async ({ page }) => {
  const httpResult = await apiContext.get("/guid/E881E5A5CCE44A5F9EB2D59F")
  expect(httpResult.status()).toBe(400);
});

test('delete guid not found failed', async ({ page }) => {
  const httpResult = await apiContext.get("/guid/E881E5A5CCE44A5F9EB2D59FAAAAAAAA")
  expect(httpResult.status()).toBe(404);
});
