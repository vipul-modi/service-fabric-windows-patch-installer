---
services: service-fabric
platforms: dotnet, windows
author: ninzavivek
---

## About this Patch
The purpose of this patch is to ensure that the MTU size of each IPv4 interface on a cluster node is set correctly. 

The MTU size of an interface programmed inside a Windows Container (NAT Networking mode) is set to 1500 bytes. MTU size mismatch between a Windows Container's interface and any of the IPv4 interfaces leads to fragmentation, which could cause choppy connections in some cases. However, an overlay network packet has an additional 50 bytes in its header. Therefore, an interface that belongs to an overlay network should have its MTU size set to 1450 and the MTU size of any other interface should be set to 1500.     

This patch starts by building a list of IDs of interfaces that belong to overlay networks. It then queries for all IPv4 interfaces on a cluster node. Using the list of IDs, it checks if an interface belongs to an overlay network or not to determine if the desired MTU size is 1450 or 1500. It then checks if the MTU size is set to less than the desired size. If this is true, it will reset the MTU value of the interface to the correct size.

Please note, this patch is only supposed to deployed on Service Fabric Clusters that have L2 tunnel network setup and have Windows container with only NAT networking mode configured.
