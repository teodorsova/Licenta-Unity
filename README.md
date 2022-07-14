# Virtual Assistant for Interior Design

This repository contains the Unity app for my bachelor thesis. There are three repositiories in total, the other two containing the Web API and the frontend.

# Introduction

What if smaller companies or workshops that procude furniture could have the possibility to upload their products on a platform that allows cutomers to view them in 3D?
This applications aims to do that in a simple, but efficient manner.

# Architecture

For the frontend part React.js was used. It also incorporates a Redux store for global state management. The frontend is also responsible for communication with the Web API and Unity (using a Unity Context). 

The Web API communicates with the relational database (PostgreSQL), which is used for managing accounts, subscriptions and orders, while the NoSQL (Firestore) is used to store data related to furniture pieces.

Azure Blob Storage hosts the furniture files, that are downloaded on-the-go when the Unity app is started.

<img width="770" alt="Drawing1" src="https://user-images.githubusercontent.com/70344140/178999028-44d388f3-4071-4efd-9411-543b5e43c24c.png">

# Usage

Both companies and users can visualise their profiles, each having different roles. Companies can manage order requests, purchase subscriptions and then upload more furniture models to the cloud, that will be used inside the Unity app, while users can use the furniture pieces to place orders.


# User perspective

### Users can access the Unity app within the React webpage and create a virtual room.

![image](https://user-images.githubusercontent.com/70344140/178997272-2dbfa298-12dd-4e33-a2fc-d9d3bf73ac8b.png)
![image](https://user-images.githubusercontent.com/70344140/178997327-3c2f089b-266a-4226-adfe-3ed84f0f1960.png)

### The user has the possibility to place an order based on the visual content of the Unity app.

![image](https://user-images.githubusercontent.com/70344140/178997706-a913acee-0dd1-4c07-8706-bc1412c0cf71.png)
![image](https://user-images.githubusercontent.com/70344140/178997804-da0a56c9-170c-48aa-aaef-3122e4d08f5f.png)

### Once an order is placed, every company that is included in the order will have the possiblity to accept or refuse it.

# Company perspective

### Company profile page:

![image](https://user-images.githubusercontent.com/70344140/178995127-a96f099e-d12c-4746-ac34-237d387db060.png)

### Adding furniture:

![image](https://user-images.githubusercontent.com/70344140/178998165-48964bb9-dcb4-46a0-9592-c0b141a74cdf.png)

### Managing orders:

![image](https://user-images.githubusercontent.com/70344140/178998351-e4399320-b39b-425e-a8e2-4fc8a82b5084.png)

When companies change the status of the orders, the users will be able to see it in real time.


