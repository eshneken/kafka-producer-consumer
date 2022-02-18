# OCI Kafka Producer-Consumer
Sample code showing how to use OCI Streaming Service with Kafka Connect enabled to produce/consume messages from .NET using
only the Confluent Kafka SDK (without any explicit OCI dependencies).

The demo shows a single producer connecting to OCI Streaming using the Kafka SDK.  It produces 5 messages.  Subsequent to this two consumers (consumer-a and consumer-b) connect.  Consumer A retrieves 2 messages followed by Consumer B which retrieves all 5 messages which is followed by Consumer A retrieving the remaining 3 messages.  This demonstrates that individual consumers can maintain their own individual 'state' and consume messages independently from a topic.

The usecase this surfaces is of loose coupling between microservices where any microservice can emit messages to any number of topics (producer) and any number of microservices with an interest in said topic can register and read messages (consumer) that are produced.

## Usage

1. In the OCI Console, create a stream using default settings.  You can choose to create a stream pool or use the default.
2. In the OCI Console, create a Kafka Connect Configuration.
3. In the OCI Console, go to your user settings and create an *Auth Token*.  Note down the token as well as your full username (i.e oracleidentitycloudservice/first.last@company.com)
4. Rename *appsettings.template.json* to *appsettings.json*.  Replace the following values:
    1. StreamingEndpoint  ->  streaming.REGION.oci.oraclecloud.com:9092 where REGION is the region that you provisioned your stream in (i.e. us-ashburn-1)
    1. Username ->  your full username
    2. Password ->  the auth token for your user
5. You may need to install the Confluent.Kafka libraries using NuGet
6. Execute *dotnet run* to run the tester.  You can validate message production in the OCI Console as well within 1 minute of publication.


## Running in Kubernetes
1. In the OCI console, create an OKE cluster with default settings. Use the *Access Cluster* menu in your new cluster to set up local cluster access.
2. Build the Docker image and push it to OCIR
    1. Log into OCIR: `docker login <region>.ocir.io -u <namespace>/<username> -p <auth token>`
    2. `docker build -t <region>.ocir.io/<namespace>/kafka-producer-consumer .`
    3. `docker push <region>.ocir.io/<namespace>/kafka-producer-consumer`
    4. Update the image name in *job.yml*
4. Create a secret for kubernetes to pull the image from OCIR:
```
kubectl create secret docker-registry ocirsecret \
--docker-server=<region>.ocir.io \
--docker-username='<namespace>/<username>' \
--docker-password='<auth-token>' \
--docker-email='<email>'
```
3. The credentials in *appsettings.json* are not included in the docker image for security reasons. Instead, you will create a kubernetes secret with the credentials, which will be injected at runtime:
```
kubectl create secret generic kafka-secret \
--from-literal=STREAMINGENDPOINT=<streaming endpoint from above> \
--from-literal=USERNAME=<username from above> \
--from-literal=PASSWORD=<auth-token>
```
4. Deploy the application to kubernetes as a job: `kubectl apply -f job.yaml`

## References:

* https://docs.confluent.io/clients-confluent-kafka-dotnet/current/overview.html
* https://docs.confluent.io/platform/current/clients/confluent-kafka-dotnet/_site/api/Confluent.Kafka.html
* https://docs.oracle.com/en-us/iaas/Content/Streaming/Tasks/kafkacompatibility_topic-Configuration.htm
