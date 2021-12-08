# K8s-LoadBalance-and-HPA
Utilizando serviços de Load Balance e Horizontal Pod Autoscaler(Escalonamento automatico de pods) de Pods do Kubernetes com Web API .NET 6

## O que e Kuberenetes?
  Kubernetes ou k8s, eh um orquestrador de container para deploy, escalonamento e manutencao dos containers. Diferente do Docker, k8s utiliza *Pods*, onde cada Pod contem um ou mais containers.
  
   O Kuberenetes trabalha com mutiplos hosts (master e nodes), onde consegue distribuir Pods automaticamente entre diferentes maquinas para melhorar performance e distribuir o poder de processamente entre as maquinas.
   
   Hoje em dia os principais servicos de cloud ja disponibilizam o K8s de uma forma transparente, com facil implementecao  e com abastracao da configuracao das maquinas, apenas sendo necessario configurar os pods que serao utilizados.

## Explicando a API
  Para esse projeto foi criado uma minimal api, utilizando uma das novidades do .NET 6, com apenas dois endpoints. Onde o endpoint ```/api/localip``` retornar o IP interno onde a aplicacao esta rodando e o endpoint ```/health``` eh utilizado para controle do Kubernetes para verificar se a aplicacao esta no ar. Mais explicacoes abaixo.

## Servicos de rede do Kubernetes?
  Dentre os diversos recusursos disponveis, o servico de rede no K8s, chamado apenas de *service*, eh um meio abstrato e facil de expor pods para a rede. Ao iniciar um pod com service o K8s ja configura um IP e um nome de DNS, com load balance automatico caso tenha replicas do mesmo Pod. A parte de configurar de rede eh realizada automaticamente pelo K8s sem possibidade de configurarcao manual, devido a isso para se comunicar entre pods eh necessario a utilizacao do nome DNS, ate pois caso seja identificado uma falha em algum Pod normalmente o K8s destroi essa instacia e cria uma nova, com novas configuracoes de IP.
  
  Abaixo os tipos de services disponiveis no kubernetes:
  
- **ClusterIP**: Expoem o service internamente para o cluster, podendo ser alcancado apenas para pods ou servicos dentro do cluster. Esse e o tipo de service padrao.
- **NodePort**: Expoem o service com uma porta estatica, por exemplo para testes se utiliza NodePort para export no localhost as portas necessarias. Caso seja implementado esse servico o ClusterIP eh automaticamente criado. obs: Portas disponiveis para expor vai de 30000-32768.
- **LoadBalancer**: Expoem o service externamente usando um load balancer de um provedor de cloud. servico de NodePort e ClusterIP sao criados automaticamente. Nesse tipo de servico nao ha restricao de porta para expor.
- **ExternalName**: Funciona como qualquer outro service, porem ao acessar o nome do servico, ao inves de retornar o IP retoranra o CNAME com o valor configurado.

## O que e HPA?
  O recuso Horizontal Pod Autoscaling (HPA) eh um recurso que escalona automaticamente, com base na carga de trabalho definida (cpu, memoria, etc), recursos de ```deployment``` ou ```statefulset``` (recursos para implementar pods com configuracoes mais avancadas), para combinar com a demanda atual requisitada.
  
  Diferente do escalonamento veritical, onde para suprir a demanda eh adicionado mais recursos como cpu ou memoria, o HPA escalona horizontamente, ou seja, cria novos pods conforme a demanda e destroi 
quando a demanda esta baica, mantendo apenas o minimo de replicas necessarias.

## Executando o projeto
Para executar o projeto inicialmente realize o clone do repositorio e acesse em seguida.
```
git clone https://github.com/VinnyNK/K8s-LoadBalance-and-HPA.git
cd K8s-LoadBalance-and-HPA
```

Para que o Kubernetes identifique a imagem que cria o Pod eh necessario buildar internamente, por padrao eh buscado a imaagem no *docker hub*. Acesse a pasta src dentro do projeto e execute o comando de build.
```
cd src
docker build -f ./PDI.K8s.LoadBalancer_HPA.MinimalApi/Dockerfile -t minimal-api:latest .
```

Apos gerado a imagem, iremos comecar a subir os recuroso do kubernetes, para isso, caso esteja em uma maquina windows, dentro das configuracoes do docker desktop ha a opcao de habilitar um cluster de kubernetes. Assim que habilitar ira ser disponibilizado o comando kubectl. 

![image](https://user-images.githubusercontent.com/28060427/145230374-df27b6ed-807c-4600-9db9-9094754edf09.png)


Asim que habilitado o cluster de k8s, volte para a raiz do projeto e acesse a pasta ```k8s```
```
cd ../k8s
```

Na pasta ira conter todos os arquivos de configuracao para subir o projeto utilizando kubernetes, separando cada arquivo por recurso.
Primeiramente iremos criar o recurso service, que seria a parte de rede. A seguir iremos executar o comando para listar os services existentes dentro do cluster.
```
kubectl apply -f deployment-minimalapi.yaml
kubectl get svc
```
![image](https://user-images.githubusercontent.com/28060427/145232217-66c8a645-f8c6-4d35-8595-b9d7dc19d05b.png)

