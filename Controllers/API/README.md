# Goynote

If you want to access the API, you'll need to be authenticated. In order to do that, send a POST request to `http://ip-address:port/api/auth/login` with a **JSON** body as follows:

```
{
  "email": "existing-user-email",
  "password": "password"
}
```

It should produce a cookie. Apps like Postman automatically save them. From now on, you should be able to access protected API endpoints