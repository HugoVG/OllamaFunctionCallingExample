

# Install

## Docker Route

### Setup docker

Keep in mind I show the docker route for NO GPU support, 
You might need a beefy CPU and a lot of RAM to run the model.
Even if you have *a* GPU, you might prefer the local route over this.

```bash
# If you are in the docker sudo group you can skip sudo
sudo docker run -d -v ollama:/root/.ollama -p 11434:11434 --name ollama ollama/ollama
```

### pull the model
```bash
# I use llama3.2:3b, this is best for my current project 
sudo docker exec -it ollama ollama pull llama3.2:3b
```

### Run the model
```bash
sudo docker exec -it ollama ollama run llama3.2:3b
```

start application as normal

## Local Route

You can follow the instructions on the [Ollama site](https://ollama.com/download/linux)

or for linux just paste

```bash
curl -fsSL https://ollama.com/install.sh | sh
```

```bash
sudo ollama pull llama3.2:3b
```

By default, ollama will run on GPU which is a ton faster than CPU.
```bash
sudo ollama run llama3.2:3b
```
The 11434 port is the default port so you must set that as the url.
