# Exodus

<div align="center">
  <h1>🚀 EXODUS PROTOCOL</h1>
  <p><i>An Interactive 3D Rescue Simulation</i></p>
  <p><b>BSc (Hons) in Computer Science | Year 3 Semester 1 | Group Assignment</b> [cite: 5]</p>
  
  <hr />

  <p align="center">
    "The city has 4 minutes before orbital bombardment. You are the last extraction unit. Rescue the stranded humans — without being caught by the rogue hover-patrol." [cite: 15]
  </p>
</div>

<br />

<h2>🏙️ Project Overview</h2>
<p>
  <b>Exodus Protocol</b> is a first-person rescue and stealth action game set in the year 2157 within the megacity of <b>Neo-Veridia</b>[cite: 15, 17]. A rogue AI has seized the city's autonomous hover-patrol fleet, and players must navigate through an oppressive, neon-lit environment to rescue civilians before an imminent orbital strike[cite: 17, 18, 100].
</p>

<ul>
  <li><b>Objective:</b> Rescue at least 5 humans to achieve mission success[cite: 39, 50].</li>
  <li><b>Time Limit:</b> 4-minute countdown until orbital bombardment[cite: 15, 18].</li>
  <li><b>Engine:</b> Developed in Unity 6 using the Universal Render Pipeline (URP)[cite: 123].</li>
</ul>

<br />

<h2>🕹️ Core Gameplay Mechanics</h2>
<table>
  <tr>
    <td><b>Movement</b></td>
    <td>First-person perspective (WASD) with walking, running, and crouching states[cite: 25, 27, 30, 31].</td>
  </tr>
  <tr>
    <td><b>Rescue System</b></td>
    <td>A physical grab-and-carry mechanic where the player must manually transport human NPCs to an evacuation beacon[cite: 20, 28, 29].</td>
  </tr>
  <tr>
    <td><b>Stealth</b></td>
    <td>No combat; players must avoid hover-car detection cones (45°) and audio triggers from sprinting[cite: 32, 46, 47].</td>
  </tr>
  <tr>
    <td><b>The Enemy</b></td>
    <td>Autonomous hover-car AI agents using NavMesh pathfinding and state-based logic (Patrol, Alert, Search)[cite: 42, 43, 44, 48].</td>
  </tr>
</table>

<br />

<h2>👥 Team Roles & Responsibilities</h2>
<p>Our group is split into four distinct specialized roles to meet the SE3032 module requirements[cite: 70]:</p>

<table width="100%">
  <thead>
    <tr>
      <th>Student</th>
      <th>Role</th>
      <th>Primary Deliverable</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><b>Student 1</b></td>
      <td>The World Builder</td>
      <td>Neo-Veridia layout, cyberpunk texturing, and baked/real-time lighting[cite: 70, 72].</td>
    </tr>
    <tr>
      <td><b>Student 2</b></td>
      <td>The Systems Engineer</td>
      <td>Player physics, NPC grab mechanics, and collision detection systems[cite: 70, 78].</td>
    </tr>
    <tr>
      <td><b>Student 3</b></td>
      <td>The Core Developer</td>
      <td>Custom 3D modeling of Hover-Cars (≤8k poly) and Human NPCs (≤5k poly)[cite: 70, 84, 117, 118].</td>
    </tr>
    <tr>
      <td><b>Student 4</b></td>
      <td>The Agent Controller</td>
      <td>AI pathfinding, state machine logic, and procedural banking animations[cite: 70, 91].</td>
    </tr>
  </tbody>
</table>

<br />

<h2>🛠️ Technical Specifications</h2>
<ul>
  <li><b>AI Pathfinding:</b> NavMesh agents offset 0.5m above ground to simulate hovering[cite: 42, 65, 92].</li>
  <li><b>Art Style:</b> A blend of Blade Runner 2049 aesthetic with brutalist structures and volumetric lighting[cite: 53, 62, 99].</li>
  <li><b>Optimization:</b> 0.02s physics timestep and LOD (Level of Detail) groups for city geometry[cite: 119, 120].</li>
  <li><b>Escape Routes:</b> Narrow alleyways are excluded from enemy NavMeshes, providing strategic safe zones[cite: 66, 67].</li>
</ul>

<br />

<h2>📅 Project Milestones</h2>
<ol>
  <li><b>Week 11:</b> Concept & Scope Pitch[cite: 114].</li>
  <li><b>Week 13:</b> Logic & Asset Prototype (Functioning grab mechanic & imported models)[cite: 114].</li>
  <li><b>Week 15:</b> Final Showcase & Viva (Complete playable build)[cite: 114].</li>
</ol>

<hr />

<div align="center">
  <p><i>Submission Deadline: 3rd May 2026</i> [cite: 6]</p>
</div>