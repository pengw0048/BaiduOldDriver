# API 概述

NetDisk 是实现百度网盘 API 的 C# 工程。使用者可以直接引用编译好的 NetDisk.dll。

目前版本的目标框架是 .NET Framework 4.5，引用它的工程最好也选用 .NET 4.5。

库中几乎所有函数的返回值以 Result 为基类。约定 bool success 表示是否成功完成调用流程（但不意味着结果为成功）。如果 success = false，Exception exception 保存了相关的异常。

*很多情况下，返回对象中给出的的 errno 记录了百度返回的操作结果，errno = 0 表示操作实际成功。*

网络传输使用 WebClient，它绝大多数情况下能自动选择合适的网络和（如果设置了的）代理服务器。

解决方案 BaiduOldDrive 中有若干使用 NetDisk 库的示例程序，可供参考。

# 身份验证

调用 Operations 类当中的操作之前，先要用这个类当中的函数获得登录信息。请将最终得到的 Credential 对象传入此后的操作函数。

它通过模拟网页版百度网盘的登录，获得 baiduid、bduss、stoken 这三个必需的 Cookies。

下面是命令行登录流程的例子：

```cs
var checkResult = Authentication.LoginCheck("用户名");
CheckSuccess(checkResult);
if (checkResult.needVCode)
{
 File.WriteAllBytes("vcode.png", checkResult.image);
 Console.WriteLine("输入 vcode.png 中的验证码：");
 checkResult.verifyCode = Console.ReadLine();
}
var loginResult = Authentication.Login("用户名", "密码", checkResult);
CheckSuccess(loginResult);
var credential = loginResult.credential;
// credential 变量以后操作使用
```

对于有 GUI 的程序，建议在用户名框更改时，就检查是否需要验证码，并对应显示。

## 登录检查

```cs
Authentication.LoginCheck(string username) : LoginCheckResult
```

在实际登录之前，需要先用这个函数，检查一下是否需要输入验证码。

### 参数

string username: 用户名

### 返回值

bool needVCode: 是否需要输入验证码

byte[] image: 如果需要验证码，这个数组存储了验证码图片

## 登录

```cs
Authentication.Login(string username, string password, LoginCheckResult checkResult) : LoginResult
```

对于同一个用户名，在调用 LoginCheck后，实际进行登录。如果 checkResult.needVCode == true，需将验证码文字存入 checkResult.verifyCode。

### 参数

string username: 用户名

string password: 密码

LoginCheckResult checkResult: 该用户名对应的 LoginCheck 返回结果

### 返回值

Credential credential: 表示登录用户的身份信息

int errno: 错误代码，详见 [这里](https://github.com/tiancaihb/BaiduOldDriver/blob/master/BaiduOldDriver/login_errno.txt)

## 检查身份信息是否有效

```cs
Authentication.IsLoggedIn(Credential credential) : bool
```

### 参数

Credential credential: 要检查的身份信息

### 返回值

是或否

# 获取基本信息

## 获取配额

```cs
Operation.GetQuota(Credential credential) : QuotaResult
```

### 参数

Credential credential: 身份信息

### 返回值

int errno: 错误代码

long total: 总字节数

long free: 空余字节数

long used: 使用字节数

## 获取用户信息

```cs
Operation.GetUserInfo(Credential credential) : UserInfoResult
```

### 参数

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string records[0].avatar_url: 头像图片的地址

string records[0].uname: 用户昵称

string records[0].priority_name: 带星号的用户昵称

# 文件操作

## 获取文件列表

```cs
Operation.GetFileList(string path, Credential credential) : FileListResult
```

对于文件夹 path，列出其下所有的文件和文件夹。根目录是“/”，其他路径后不加斜线，例如“/Folder”。

### 参数

string path: 网盘上的文件夹全路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

FileListResult.Entry[] list: 文件（夹）信息的数组

int list.isdir: 是否为文件夹（1 或 0）

string list.path: 该项目的全路径

string list.server_filename: 该项目的名称

long list.size: 字节数

## 获得缩略图

```cs
Operation.GetThumbnail(string path, Credential credential, int width = 125, int height = 90, int quality = 100) : ThumbnailResult
```

### 参数

string path: 网盘上的文件全路径

Credential credential: 身份信息

int width: 宽度

int height: 高度

int quality: 质量

### 返回值

byte[] image: 缩略图文件

## 复制 / 移动

```cs
Operation.Copy(string path, string dest, string newname, Credential credential) : FileOperationResult
Operation.Move(string path, string dest, string newname, Credential credential) : FileOperationResult
```

如果指定的名字已经存在，会返回错误。

### 参数

string path: 网盘上的文件（夹）全路径

string dest: 目标所在文件夹的全路径

string newname: 新的名字

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 原文件（夹）的全路径

## 重命名

```cs
Operation.Rename(string path, string newname, Credential credential) : FileOperationResult
```

如果指定的名字已经存在，会返回错误。

### 参数

string path: 网盘上的文件（夹）全路径

string newname: 新的名字

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 原文件（夹）的全路径

## 删除

```cs
Operation.Delete(string path, Credential credential) : FileOperationResult
```

### 参数

string path: 网盘上的文件（夹）全路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 原文件（夹）的全路径

## 新建文件夹

```cs
Operation.CreateFolder(string path, Credential credential) : FileOperationResult
```

如果指定的名字已经存在，会自动改名，例如"/Folder(1)"。

### 参数

string path: 网盘上的文件夹全路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 实际创建的文件夹全路径

# 上传下载

## 获取下载地址

```cs
Operation.GetDownload(string path, Credential credential) : GetDownloadResult
```

对于一个文件通常会返回一组地址，从域名可以看出是不同地区的服务器。下载速度有时差别会很大，特别是从国外下载的时候；此外返回的 rank 不一定符合实际情况。这些地址的有效期是 8 小时。

百度网盘客户端对于非会员有某种限速，但给会员的下载地址可能也会比非会员的更快。具体情况还需进一步实验。

### 参数

string path: 网盘上的文件夹全路径

Credential credential: 身份信息

### 返回值

GetDownloadResult.Entry[] urls: 下载地址的数组

int urls.rank: 百度给的服务器优先度排名

string urls.url: 下载地址

## 简单上传

```cs
Operation.SimpleUpload(string localpath, string remotepath, Credential credential, string host = "c.pcs.baidu.com") : CommitUploadResult
```

适合小文件上传。因为实现当中会先把文件全部读入内存，并且一次 WebClient 操作的超时有限，对于较大（例如 >= 10MB）的文件，请考虑分块上传。

目前这个函数仍然是通过分块上传实现的。

### 参数

string localpath: 本地文件全路径

string remotepath: 网盘上的文件保存路径

Credential credential: 身份信息

string host: 上传服务器的地址，可以用 GetUploadServers 函数获取

### 返回值

int errno: 错误代码

其他成员类似 FileListResult.Entry

## 分块上传（简化版）

```cs
Operation.ChunkedUpload(string localpath, string remotepath, Credential credential) : CommitUploadResult
```

在内部使用 4MB 大小的分块上传指定文件。该函数会自动获取并选择第一个上传服务器。

### 参数

string localpath: 本地文件全路径

string remotepath: 网盘上的文件保存路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

其他成员类似 FileListResult.Entry

## 上传准备：获取文件哈希值

```cs
UploadHelper.GetFileProperty(string path) : FileProperty
```

在使用其他上传函数之前，需要先调用此函数，计算分块和秒传所需的分块哈希值等信息。

### 参数

string path: 本地文件全路径

### 返回值

记录文件哈希值的对象

## 极速秒传

```cs
Operation.RapidUpload(FileProperty prop, string path, Credential credential) : RapidUploadResult
```

通过提供文件的 MD5、CRC32、开头的 MD5、每个分块的 MD5，由服务器查询是否已经有此文件，从而实现“极速秒传”。失败则 errno = 404。

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

FileListResult.Entry info: 若成功上传，该文件的信息

## 初始化分块上传

```cs
Operation.InitUpload(FileProperty prop, string path, Credential credential) : InitUploadResult
```

在计算好一个文件的哈希信息后，文件的分块上传分为三个步骤：初始化，上传每个分块，以及提交。首先，调用此函数，上传文件的分块信息以及获得上传会话 ID。有可能服务器上已经有某些分块了，所以 block_list 不包括所有分块的编号。

使用方法可以参见 Operation.ChunkedUpload 的实现。

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string uploadid: 上传会话的 ID

int[] block_list: 需要上传的分块编号

## 上传分块内容

```cs
Operation.UploadBlock(FileProperty prop, string path, InitUploadResult session, FileStream stream, int blockid, string host, Credential credential)
```

初始化完成后，由调用者打开一个能读取的 FileStream，这样每次上传一个分块时可以重用。

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

InitUploadResult session: 初始化得到的结果

FileStream stream: 供读取的文件流

int blockid: 块的序号

string host: 上传服务器的地址，可以用 GetUploadServers 函数获取

Credential credential: 身份信息

### 返回值

由 bool success 表示是否成功

## 提交分块上传

```cs
Operation.CommitUpload(FileProperty prop, string path, InitUploadResult session, Credential credential) : CommitUploadResult
```

上传完成所有

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

InitUploadResult session: 初始化得到的结果

Credential credential: 身份信息

### 返回值

int errno: 错误代码

其他成员类似 FileListResult.Entry

## 获得上传服务器列表

```cs
Operation.GetUploadServers(Credential credential) : GetUploadServersResult
```

上传完成所有

### 参数

Credential credential: 身份信息

### 返回值

string[] servers: 服务器域名列表


