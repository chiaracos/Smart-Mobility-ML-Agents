# Smart Mobility ML-Agents

Progetto di **reinforcement learning** sviluppato con **Unity ML-Agents** che addestra un agente veicolare autonomo a navigare in un ambiente urbano simulato. L'agente impara a seguire un percorso, rispettare i limiti di velocità e obbedire ai semafori — tutto tramite trial and error, senza alcuna regola programmata manualmente.

---

## Obiettivo

Addestrare un agente virtuale (`CarAgent`) a guidare autonomamente in una simulazione Unity, replicando aspetti chiave della guida urbana reale:

- **Seguire un percorso stradale** senza uscire dalla carreggiata
- **Rispettare i semafori** — fermarsi al rosso, ripartire al verde
- **Rispettare i limiti di velocità** — adattare la velocità in base ai segnali stradali

L'agente apprende tutto questo da zero utilizzando l'algoritmo **PPO (Proximal Policy Optimization)**, guidato unicamente da un segnale di ricompensa.

---

Il progetto utilizza il toolkit **Unity ML-Agents**, che collega le simulazioni Unity al deep reinforcement learning basato su Python.

L'agente:
- **Osserva** l'ambiente: posizione, velocità, stato del semaforo, limiti di velocità rilevati
- **Agisce** controllando accelerazione, frenata e sterzo
- **Riceve ricompense** per restare in carreggiata, fermarsi ai semafori rossi e rispettare i limiti
- **Viene penalizzato** per bruciare i semafori, superare i limiti di velocità o uscire dal percorso
- **Impara** iterando su centinaia di migliaia di episodi tramite PPO

È integrato anche un **modulo di curiosità** per incentivare l'esplorazione e aiutare l'agente a scoprire strategie migliori in situazioni complesse o con reward sparso.

---

## Tecnologie utilizzate

| Strumento | Ruolo |
|-----------|-------|
| Unity | Ambiente di simulazione (strada, semafori, segnali) |
| ML-Agents (Python) | Framework di training RL |
| PPO | Algoritmo di addestramento |
| C# | Logica dell'agente, scripting ambiente, rilevamento segnali |
| YAML | Configurazione degli iperparametri |

---

## Configurazione del training

Il training è configurato tramite `car_config.yaml`:

```yaml
behaviors:
  CarAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 64
      buffer_size: 12000
      learning_rate: 0.0001
      beta: 0.01
      epsilon: 0.1
      lambd: 0.95
      num_epoch: 3
    network_settings:
      normalize: true
      hidden_units: 128
      num_layers: 2
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
      curiosity:
        gamma: 0.99
        strength: 0.02
        encoding_size: 256
        learning_rate: 3.0e-4
    max_steps: 700000
    time_horizon: 64
    summary_freq: 1000
```

Scelte principali:
- **PPO** per un training stabile ed efficiente su task multi-obiettivo
- **Reward di curiosità** per favorire l'esplorazione in scenari complessi (percorso + semafori + velocità)
- **700.000 step massimi** per una convergenza solida su più obiettivi simultanei
- **Normalizzazione degli input** per migliorare le prestazioni della rete neurale

---

## Come eseguire il progetto

### Prerequisiti
- Unity (2021.3+ consigliato)
- Python 3.8+
- Pacchetto ML-Agents: `pip install mlagents`

### Avviare il training

```bash
mlagents-learn car_config.yaml --run-id=CarAgentTraining
```

Poi premere **Play** nell'Unity Editor per avviare la simulazione.

### Visualizzare i risultati

I risultati e i log TensorBoard vengono salvati in:
```
results/CarAgentTraining/
```

Per visualizzare l'andamento del training:
```bash
tensorboard --logdir results
```

---

## Struttura del progetto

```
Smart-Mobility-ML-Agents/
├── Assets/                   # Scena Unity, script agente, ambiente
│   ├── Scripts/              # Logica C# agente, rilevamento semafori e segnali
│   ├── Scenes/               # Scene di simulazione Unity
│   └── Prefabs/              # Auto, semafori, strada, segnali limite velocità
├── Packages/                 # Dipendenze Unity
├── ProjectSettings/          # Impostazioni progetto Unity
├── results/CarAgentTraining/ # Output training e log TensorBoard
├── car_config.yaml           # Configurazione iperparametri ML-Agents
└── .gitignore
```

---


Sviluppato nell'ambito di un progetto universitario che esplora l'applicazione del **reinforcement learning** a scenari di **smart mobility**. L'agente deve gestire simultaneamente più regole del codice della strada — mantenimento della corsia, rispetto dei semafori e dei limiti di velocità — rendendolo un problema di RL multi-obiettivo in una simulazione urbana realistica.

---

