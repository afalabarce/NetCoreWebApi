# NetCoreWebApi
Typical and simple .Net Core WebApi REST service, to use as a starter project.

Implements JWT token authentication against a database (Sql Server, PostgreSql and MySql engines are supported).

Some example RSA keys are generated, using the online RSA key generation website.

Authentication is done in two phases:
1. The login and password are sent encrypted with the public key (bearer authentication header format <encrypted user:password string> to the api/Auth Controller.
2. If the credentials are valid, a token is obtained in the x-authjwttoken header, which will be used in each webservice call (new tokens are always generated in each request).

if you think I've done a good job, and you think I deserve a coffee, you can invite me with [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/R5R4NB8VV)
