# Self-Driving Car Simulation

An autonomous vehicle simulation in Unity trained using Reinforcement Learning via the Unity ML-Agents toolkit. The agent is trained to navigate a complex track, avoid obstacles using edge sensors, and continuously optimize driving speed and steering accuracy.

## Project Structure

The project code is organized inside the standard Unity structure under `Assets/Scripts/`:

### Core Components
*   **`CarAgent` (`Assets/Scripts/Car/CarAgent.cs`)**: The primary Agent class that handles reward calculation, steering/acceleration inputs, sensor evaluations, and resetting upon training failures (crashing/stagnation).
*   **`CarEdgeSensors` (`Assets/Scripts/Car/CarEdgeSensors.cs`)**: Implements raycast-based distance calculations to detect track boundaries and guide the agent.
*   **`PresentationAutoPilot` (`Assets/Scripts/Car/PresentationAutoPilot.cs`)**: Allows running a demo or presentation-friendly PID/heuristic autopilot navigation.
*   **`CheckpointRecorder` (`Assets/Scripts/Car/CheckpointRecorder.cs`)**: Tracks checkpoint progress and prevents "checkpoint farming" behavior.
*   **`CameraFollow` (`Assets/Scripts/Camera/CameraFollow.cs`)**: Smoothly captures and tracks the agent's drive simulation from a fixed height offset.
*   **`CheckpointAutoAssigner` (`Assets/Scripts/Editor/CheckpointAutoAssigner.cs`)**: Editor helper tool to quickly configure checkpoints sequentially on the track.
*   **`TestCarAgent` (`Assets/Scripts/Car/TestCarAgent.cs`)**: Agent variant specifically built for code testing and hyperparameter verification.

---

## Configuration

ML-Agents hyperparameter tuning is configured via the trainer configuration:
*   File path: [`trainer_config.yaml`](./trainer_config.yaml)
*   Configures PPO (Proximal Policy Optimization) hyper-parameters, state-space settings, reward functions, and buffer allocations.

---

## Setup & Training Guide

### 1. Prerequisites
- **Unity Editor** (compatible version installed with ML-Agents package support).
- **Python 3.10** with ML-Agents installed.

### 2. Environment Setup
To initialize the local python environment, run from the repository root:
```bash
# Activate python environment
.\mlagents-env310\Scripts\activate
```

### 3. Run Training
To start a new training run:
```bash
mlagents-learn trainer_config.yaml --run-id=CarDrivingSim_Run01
```

Once started, press **Play** in the Unity Editor to link the simulation and begin training.

### 4. Viewing Results
TensorBoard can be used to observe the training metrics (e.g. cumulative rewards, episode duration):
```bash
tensorboard --logdir results
```
Checkpoints and exported neural network structures will be saved under the [`results/`](./results/) folder as `.onnx` models.