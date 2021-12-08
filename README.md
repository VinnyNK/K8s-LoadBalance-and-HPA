# K8s-LoadBalance-and-HPA
Utilizando serviços de Load Balance e Horizontal Pod Autoscaler(Escalonamento automático de pods) de Pods do Kubernetes com Web API .NET 6

## O que e Kuberenetes?
  Kubernetes ou k8s, eh um orquestrador de container para deploy, escalonamento e manutenção dos containers. Diferente do Docker, k8s utiliza *Pods*, onde cada Pod contem um ou mais containers.
  
   O Kuberenetes trabalha com múltiplos hosts (master e nodes), onde consegue distribuir Pods automaticamente entre diferentes maquinas para melhorar performance e distribuir o poder de processamento entre as maquinas.
   
   Hoje em dia os principais serviços de cloud já disponibilizam o K8s de uma forma transparente, com fácil implementação e com abstração da configuração das maquinas, apenas sendo necessário configurar os pods que serão utilizados.

## Explicando a API
  Para esse projeto foi criado uma minimal api, utilizando uma das novidades do .NET 6, com apenas dois endpoints. Onde o endpoint ```api/localip``` retornar o IP interno onde a aplicação esta rodando e o endpoint ```/health``` eh utilizado para controle do Kubernetes para verificar se a aplicação esta no ar.

## Servicos de rede do Kubernetes?
  Dentre os diversos recursos disponíveis, o serviço de rede no K8s, chamado apenas de *service*, eh um meio abstrato e fácil de expor pods para a rede. Ao iniciar um pod com service o K8s já configura um IP e um nome de DNS, com load balance automático caso tenha replicas do mesmo Pod. A parte de configurar de rede eh realizada automaticamente pelo K8s sem possibilidade de configuração manual, devido a isso para se comunicar entre pods eh necessário a utilização do nome DNS, ate pois caso seja identificado uma falha em algum Pod normalmente o K8s destrói essa instância e cria uma nova, com novas configurações de IP.
  
  Abaixo os tipos de services disponíveis no kubernetes:
  
- **ClusterIP**: Expõem o service internamente para o cluster, podendo ser alcançado apenas para pods ou serviços dentro do cluster. Esse e o tipo de service padrão.
- **NodePort**: Expõem o service com uma porta estática, por exemplo para testes se utiliza NodePort para exportar no localhost as portas necessárias. Caso seja implementado esse serviço o ClusterIP eh automaticamente criado. obs: Portas disponíveis para expor vai de 30000-32768.
- **LoadBalancer**: Expõem o service externamente usando um load balancer de um provedor de cloud. Serviço de NodePort e ClusterIP são criados automaticamente. Nesse tipo de serviço não ha restrição de porta para expor.
- **ExternalName**: Funciona como qualquer outro service, porem ao acessar o nome do serviço, ao invés de retornar o IP retornara o CNAME com o valor configurado.

## O que eh HPA?
  O recuso Horizontal Pod Autoscaling (HPA) eh um recurso que escalona automaticamente, com base na carga de trabalho definida (cpu, memoria, etc), recursos de ```deployment``` ou ```statefulset``` (recursos para implementar pods com configurações mais avançadas), para combinar com a demanda atual requisitada.
  
  Diferente do escalonamento vertical, onde para suprir a demanda eh adicionado mais recursos como cpu ou memoria, o HPA escalona horizontalmente, ou seja, cria novos pods conforme a demanda e destrói
quando a demanda esta baica, mantendo apenas o mínimo de réplicas necessárias.

## Executando o projeto
Para executar o projeto inicialmente realize o clone do repositório e acesse em seguida.
```
git clone https://github.com/VinnyNK/K8s-LoadBalance-and-HPA.git
cd K8s-LoadBalance-and-HPA
```

Para que o Kubernetes identifique a imagem que cria o Pod eh necessário buildar internamente, por padrão eh buscado a imagem no *docker hub*. Acesse a pasta src dentro do projeto e execute o comando de build.
```
cd src
docker build -f ./PDI.K8s.LoadBalancer_HPA.MinimalApi/Dockerfile -t minimal-api:latest .
```

Apos gerado a imagem, iremos começar a subir os recurso do kubernetes, para isso, caso esteja em uma maquina windows, dentro das configurações do docker desktop ha a opção de habilitar um cluster de kubernetes. Assim que habilitar ira ser disponibilizado o comando kubectl.

![image](https://user-images.githubusercontent.com/28060427/145230374-df27b6ed-807c-4600-9db9-9094754edf09.png)

Assim que habilitado o cluster de k8s, volte para a raiz do projeto e acesse a pasta ```k8s```
```
cd ../k8s
```

Na pasta ira conter todos os arquivos de configuração para subir o projeto utilizando kubernetes, separando cada arquivo por recurso.
Primeiramente iremos criar o recurso service, que seria a parte de rede. A seguir iremos executar o comando para listar os services existentes dentro do cluster.
```
kubectl apply -f svc-minimal-api.yaml
kubectl get svc
```
![image](https://user-images.githubusercontent.com/28060427/145243424-2593587f-db2c-47f5-8c47-bae4e53b3121.png)

Agora iremos iniciar o recurso deployment, onde ha a informação do(s) Pod(s) além de configuração de número de réplicas, recurso de health cheack e liveness probe, e qual o limite do recurso do pos. Apos para analisar os deployments no cluster e em seguida o comando que lista os pods no cluster.
```
kubectl apply -f deployment-minimalapi.yaml
kubectl get deployments
```
![image](https://user-images.githubusercontent.com/28060427/145243684-19b10110-8de3-4e92-b98b-236d8d78f82b.png)

Antes de iniciar o HPA, eh necessário iniciar o servidor de métricas para o k8s conseguir analisar os recursos de cada pod e saber quando criar novos pods ou destruir caso necessário.
```
cd metric-server
kubectl apply -f components.yaml
```
![image](https://user-images.githubusercontent.com/28060427/145244070-7fbfb6ba-1c46-49d3-8907-03d0139bd9ae.png)

Agora podemos iniciar o HPA para começar a analisar o deployment que o mesmo foi configurado. Abaixo ha o comando para listar os HPA do cluster. Normalmente logo apos iniciar o HPA o mesmo demora um tempo para começar a coletar as métricas do pod, podendo aparecer unknow conforme imagem.
```
cd ..
kubectl apply -f hpa-deployment-minimalapi.yaml
kubectl get hpa
```
![image](https://user-images.githubusercontent.com/28060427/145244486-724119ed-57b0-46cb-b19f-a98e3e61ca2f.png)


Apos um tempo o mesmo começara a buscar as métricas.

![image](https://user-images.githubusercontent.com/28060427/145244649-8d5c6033-9c76-42a2-9780-4d0891d377b0.png)

### Teste de stress
  Para conseguirmos testar o Loadbalancer e a criação de novos pods, na pasta k8s ha um script shell que executa diversas requisições com output para observarmos os IPs alterando.
  ```
  como estou executando no windows, estou utilizando o gitbash para executar esse script
  
  ./stress.sh 0.001
  ```
  
  Assim que o script iniciar podemos executar o comando que lista o HPA com a tag ```--watch``` para observarmos o número de pods aumentando e diminuindo apos parar o script, além da porcentagem alvo configurada.
  
  ```
  kubectl get hpa --watch
  ```
  
Conforme imagem abaixo, podemos acompanhar o número de deployments na coluna replica.

![image](https://user-images.githubusercontent.com/28060427/145255073-936400ab-a030-4120-8ebd-adfb58a3299f.png)

  
  Assim que parar o script, você pode analisar o arquivo criado ```out.txt``` onde informa o retorno da API, no começa o retorno eh apenas um IP, porem assim que o HPA começa a gerar novos deployments, podemos ver o loadbalance funcionando e alternando o retorno, ou seja, alterando a requisição entre os pods disponíveis.
  
  ![image](https://user-images.githubusercontent.com/28060427/145255239-fd843fce-59ad-4ccd-b140-88b74bb82987.png)

